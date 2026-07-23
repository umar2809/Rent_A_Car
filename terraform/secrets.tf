# Stores the database password encrypted, instead of plain text in systemd files or scripts.
# SSM Parameter Store SecureString is free, unlike AWS Secrets Manager which charges per secret.
resource "aws_ssm_parameter" "db_password" {
  name        = "/${var.project_name}/db_password"
  description = "RDS master password, encrypted at rest"
  type        = "SecureString"
  value       = var.db_password

  tags = {
    Name = "${var.project_name}-db-password"
  }
}

# Lets the EC2 instance read this one specific parameter, nothing else, least privilege
resource "aws_iam_role_policy" "read_db_password" {
  name = "${var.project_name}-read-db-password"
  role = aws_iam_role.ec2_ssm.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect   = "Allow"
      Action   = ["ssm:GetParameter"]
      Resource = aws_ssm_parameter.db_password.arn
    }]
  })
}
