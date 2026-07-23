#!/bin/bash
# Runs automatically on first boot, installs what the dotnet app needs to run
# The app itself is deployed manually afterward through an SSM session, see the README
dnf update -y
dnf install -y git nginx amazon-ssm-agent

# Make sure the SSM agent is actually running, some AMI builds don't auto start it
systemctl enable amazon-ssm-agent
systemctl start amazon-ssm-agent

# Install the dotnet 9 runtime, ASP.NET Core runtime specifically since this is a web api
curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
chmod +x /tmp/dotnet-install.sh
/tmp/dotnet-install.sh --channel 9.0 --runtime aspnetcore --install-dir /usr/share/dotnet

# Make dotnet available system wide
ln -sf /usr/share/dotnet/dotnet /usr/bin/dotnet

# Nginx acts as a reverse proxy, sits on port 80, forwards to the app on port 5000
cat > /etc/nginx/conf.d/rentacar.conf << 'EOF'
server {
    listen 80;

    location / {
        proxy_pass http://127.0.0.1:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    }
}
EOF

systemctl enable nginx
systemctl start nginx