resource "keycloak_oidc_identity_provider" "azidiridp" {
  realm                         = data.keycloak_realm.hg_realm.id
  alias                         = "azureidir"
  display_name                  = "Azure IDIR"
  enabled                       = true
  store_token                   = false
  trust_email                   = true
  first_broker_login_flow_alias = keycloak_authentication_flow.first_login.alias
  sync_mode                     = "FORCE"
  authorization_url             = "${var.environment.base_url}/realms/standard/protocol/openid-connect/auth"
  token_url                     = "${var.environment.base_url}/realms/standard/protocol/openid-connect/token"
  logout_url                    = "${var.environment.base_url}/realms/standard/logout/protocol/openid-connect"
  backchannel_supported         = true
  user_info_url                 = "${var.environment.base_url}/realms/standard/protocol/openid-connect/userinfo"
  client_id                     = var.keycloak_idp_azure_idir_client.id
  client_secret                 = var.keycloak_idp_azure_idir_client.secret
  issuer                        = "${var.environment.base_url}/realms/standard"
  default_scopes                = "openid profile email"
  validate_signature            = true

  jwks_url = "${var.environment.base_url}/realms/standard/protocol/openid-connect/certs"
  extra_config = {
    "clientAuthMethod" = "client_secret_post"
    "prompt"           = "login"
  }
  depends_on = [
    keycloak_authentication_flow.first_login
  ]
}

resource "keycloak_user_template_importer_identity_provider_mapper" "az_username_importer" {
  realm                   = data.keycloak_realm.hg_realm.id
  name                    = "username"
  identity_provider_alias = keycloak_oidc_identity_provider.azidiridp.alias
  template                = "$${CLAIM.idir_username}@idir"
  extra_config = {
    syncMode = "INHERIT"
  }
}

resource "keycloak_attribute_importer_identity_provider_mapper" "az_idir_userid" {
  realm                   = data.keycloak_realm.hg_realm.id
  name                    = "idir_userid"
  claim_name              = "idir_username"
  identity_provider_alias = keycloak_oidc_identity_provider.azidiridp.alias
  user_attribute          = "idir_userid"
  extra_config = {
    syncMode = "INHERIT"
  }
}

resource "keycloak_attribute_importer_identity_provider_mapper" "az_idir_guid" {
  realm                   = data.keycloak_realm.hg_realm.id
  name                    = "idir_guid"
  claim_name              = "idir_user_guid"
  identity_provider_alias = keycloak_oidc_identity_provider.azidiridp.alias
  user_attribute          = "idir_guid"
  extra_config = {
    syncMode = "INHERIT"
  }
}

resource "keycloak_attribute_importer_identity_provider_mapper" "az_displayname" {
  realm                   = data.keycloak_realm.hg_realm.id
  name                    = "displayName"
  claim_name              = "display_name"
  identity_provider_alias = keycloak_oidc_identity_provider.azidiridp.alias
  user_attribute          = "displayName"
  extra_config = {
    syncMode = "INHERIT"
  }
}

resource "keycloak_hardcoded_attribute_identity_provider_mapper" "az_idp" {
  realm                   = data.keycloak_realm.hg_realm.id
  name                    = "idp"
  identity_provider_alias = keycloak_oidc_identity_provider.azidiridp.alias
  attribute_name          = "idp"
  attribute_value         = "IDIR"
  user_session            = false
  extra_config = {
    syncMode = "INHERIT"
  }
}