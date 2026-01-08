resource "mongodbatlas_project" "this" {
  name   = "order-ms-mongo-db"
  org_id = var.mongodb_atlas_org_id
}

resource "mongodbatlas_cluster" "this" {
  project_id = mongodbatlas_project.this.id
  name       = "order-ms-mongo-db-cluster"

  provider_name               = "TENANT"
  backing_provider_name       = "AWS"
  provider_region_name        = "US_EAST_1"
  provider_instance_size_name = "M0"

  cluster_type = "REPLICASET"
}

resource "mongodbatlas_database_user" "this" {
  project_id = mongodbatlas_project.this.id

  username = var.db_username
  password = var.db_password
  auth_database_name = "admin"

  roles {
    role_name     = "readWrite"
    database_name = "OrderDb"
  }
}

resource "mongodbatlas_project_ip_access_list" "allow_all_dev" {
  project_id = mongodbatlas_project.this.id
  cidr_block = "0.0.0.0/0"
  comment    = "DEV ONLY - Allow all"
}