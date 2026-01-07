# Exporta o endpoint do banco de dados após a criação
output "db_endpoint" {
  description = "O endpoint da instância do banco de dados RDS."
  value       = aws_db_instance.sqlserver_payment_ms.address
}

output "private_subnet_ids" {
  value = data.aws_subnets.private_subnets.ids
}

output "rds_security_group_id" {
  description = "The ID of the security group for the RDS instance."
  value       = data.terraform_remote_state.network.outputs.rds_security_group_id
}