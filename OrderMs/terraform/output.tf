output "private_subnet_ids" {
  value = data.aws_subnets.private_subnets.ids
}

output "mongodb_connection_string" {
  value = mongodbatlas_cluster.this.connection_strings[0].standard_srv
}