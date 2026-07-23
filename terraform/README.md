# Terraform: VPC, EC2, and RDS for the Rent A Car dotnet app

## What this builds

One VPC. One public subnet. Two private subnets (RDS needs two availability zones for its subnet group, this is an AWS rule, not extra work you asked for). Internet gateway for the public subnet. NAT gateway so the private subnets get outbound internet without being exposed. One EC2 instance in the private subnet, prepped with the dotnet runtime, git, and nginx as a reverse proxy. One small MySQL RDS instance in the private subnets, only reachable from the EC2 box. Once this Terraform apply finishes, the actual Rent A Car app gets deployed onto the EC2 box manually through an SSM session, systemd service steps are in the deployment section below.

## Before you start

You need three things installed:

1. Terraform (version 1.5 or newer)
2. AWS CLI, configured with a real account (run `aws configure` and put in your access key)
3. Session Manager plugin for AWS CLI, this lets you connect to the private EC2 box without SSH keys or open ports

## Files in this folder

providers.tf sets the AWS provider and region.
variables.tf holds every setting you can change.
vpc.tf builds the network, subnets, gateways, routes.
security_groups.tf locks down who can talk to what.
ec2.tf builds the app server and its permission to use Session Manager.
user_data.sh is the script that installs the web app when the server boots.
rds.tf builds the small database.
outputs.tf shows you the important ids and endpoint after apply.

## Steps to run it

Step one, open a terminal in this folder.

Step two, run this to download the AWS provider:
```
terraform init
```

Step three, check what will get built, and give it a database password:
```
terraform plan -var="db_password=YourStrongPassword123"
```

Step four, if the plan looks right, apply it:
```
terraform apply -var="db_password=YourStrongPassword123"
```
Type yes when it asks. This takes around ten minutes mostly because of the NAT gateway and RDS.

Step five, connect to your EC2 box without any SSH key:
```
aws ssm start-session --target <ec2_instance_id from output>
```

Step six, once connected, test the app locally:
```
curl localhost
```
You should see the practice app html.

Step seven, when done practicing, tear everything down so you do not get billed:
```
terraform destroy -var="db_password=YourStrongPassword123"
```

## Deploying the actual app after terraform apply

Step one, connect to the box:
```
aws ssm start-session --target <ec2_instance_id from output>
```

Step two, clone your repo, use the feature branch or main once merged:
```
cd /opt
sudo git clone https://github.com/umar2809/Rent_A_Car.git
cd Rent_A_Car
```

Step three, publish the app:
```
sudo dotnet publish -c Release -o /opt/rentacar-app
```

Step four, fetch the real database password from Parameter Store, it never touches a plain text file this way:
```
DB_PASS=$(aws ssm get-parameter --name "/rentacar/db_password" --with-decryption --query Parameter.Value --output text --region ap-south-1)
```

Step five, run it once manually to confirm it starts clean:
```
cd /opt/rentacar-app
sudo ConnectionStrings__RentalDb="server=<rds_endpoint>;port=3306;database=rentacar;user=appadmin;password=$DB_PASS" dotnet Rent_A_Car.dll
```
Ctrl + C to stop once you see it listening on port 5000 with no errors.

Step six, set it up as a systemd service so it survives reboots and restarts automatically. This version pulls the password fresh from Parameter Store at boot instead of storing it in the unit file itself:
```
sudo tee /etc/systemd/system/rentacar.service << 'EOF'
[Unit]
Description=Rent A Car dotnet app
After=network.target

[Service]
WorkingDirectory=/opt/rentacar-app
ExecStartPre=/bin/bash -c 'echo "ConnectionStrings__RentalDb=server=<rds_endpoint>;port=3306;database=rentacar;user=appadmin;password=$(/usr/local/bin/aws ssm get-parameter --name /rentacar/db_password --with-decryption --query Parameter.Value --output text --region ap-south-1)" > /opt/rentacar-app/.env'
EnvironmentFile=/opt/rentacar-app/.env
ExecStart=/usr/bin/dotnet /opt/rentacar-app/Rent_A_Car.dll
Restart=always
RestartSec=10
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000

[Install]
WantedBy=multi-user.target
EOF

sudo systemctl daemon-reload
sudo systemctl enable rentacar
sudo systemctl start rentacar
sudo systemctl status rentacar
```
Note, the AWS CLI needs to be on the box for that ExecStartPre line to work, install it if it is missing, `sudo dnf install -y aws-cli`, or `curl` the official installer if that package name is not found on Amazon Linux 2023.

Step seven, confirm it works:
```
curl localhost
```
Should hit nginx on port 80, which forwards to your app on port 5000.

## Free tier notes, read this before applying

Region is set to ap-south-1, Mumbai.

EC2 t3.micro and RDS db.t3.micro are both free tier eligible, 750 hours a month each, only if your AWS account is within its first 12 months. Check this yourself in the AWS Billing console under Free Tier usage if unsure.

The NAT gateway is not free tier eligible, ever, on any account age. It bills roughly 0.045 dollars per hour plus data processed, so leaving it running costs a small but real amount. For practice, apply, test, destroy in the same session, do not leave it running overnight.

## Cost note

NAT gateway and RDS both cost money per hour even when idle. This setup uses the smallest sizes (t3.micro, db.t3.micro) but is not free tier for NAT gateway. Destroy it when you are done practicing for the day.

## What to try next once comfortable

Move the database password into AWS Secrets Manager instead of a plain variable.
Add a second EC2 in private subnet b and put a load balancer in the public subnet.
Add an S3 bucket and give the EC2 role permission to read from it.
Split this into separate modules once you are comfortable with a single flat folder.

## One recommended action

Run terraform plan first, read every line of what it says it will create, before you apply. This is the single habit that separates beginners from people who get paged at 3am for an unexpected NAT gateway bill.
