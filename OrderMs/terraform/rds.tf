resource "aws_db_instance" "sqlserver_payment_ms" {
  identifier = "goncalvesc97-fiap-tc-4-payment-ms-db"
  engine = "sqlserver-ex"
  instance_class    = "db.t3.micro"
  allocated_storage = 20
  username = "admin"
  password = var.db_password
  db_subnet_group_name  = data.aws_db_subnet_group.rds_subnet_group.name
  skip_final_snapshot = true

  # Conecta ao Security Group criado
  vpc_security_group_ids = [data.terraform_remote_state.network.outputs.rds_security_group_id]

  # Configurações adicionais
  publicly_accessible = true  # Permite acesso via internet.
  multi_az            = false # Para alta disponibilidade. Mudar para true em produção.
  storage_type        = "gp2"

  tags = {
    Name = "payment-ms-sqlserver-db"
  }
}