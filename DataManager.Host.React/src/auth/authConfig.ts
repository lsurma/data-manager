import { type Configuration, LogLevel } from "@azure/msal-browser";

// Check if authentication is enabled
export const isAuthEnabled = import.meta.env.VITE_ENABLE_AUTH === "true";

// Validate required environment variables only if authentication is enabled
const clientId = import.meta.env.VITE_AZURE_CLIENT_ID;
const tenantId = import.meta.env.VITE_AZURE_TENANT_ID;

if (isAuthEnabled) {
  if (!clientId || clientId === "your-client-id") {
    console.warn("⚠️ MSAL: VITE_AZURE_CLIENT_ID is not configured. Please set it in your .env file.");
  }

  if (!tenantId || tenantId === "your-tenant-id") {
    console.warn("⚠️ MSAL: VITE_AZURE_TENANT_ID is not configured. Please set it in your .env file.");
  }
  
  console.info("✅ Authentication is ENABLED");
} else {
  console.info("ℹ️ Authentication is DISABLED");
}

/**
 * Configuration object to be passed to MSAL instance on creation. 
 * For a full list of MSAL.js configuration parameters, visit:
 * https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-browser/docs/configuration.md 
 */
export const msalConfig: Configuration = {
  auth: {
    clientId: clientId || "your-client-id",
    authority: `https://login.microsoftonline.com/${tenantId || "your-tenant-id"}`,
    redirectUri: import.meta.env.VITE_REDIRECT_URI || window.location.origin,
    postLogoutRedirectUri: import.meta.env.VITE_POST_LOGOUT_REDIRECT_URI || window.location.origin,
  },
  cache: {
    cacheLocation: "sessionStorage", // This configures where your cache will be stored
  },
  system: {
    loggerOptions: {
      loggerCallback: (level, message, containsPii) => {
        if (containsPii) {
          return;
        }
        switch (level) {
          case LogLevel.Error:
            console.error(message);
            return;
          case LogLevel.Info:
            console.info(message);
            return;
          case LogLevel.Verbose:
            console.debug(message);
            return;
          case LogLevel.Warning:
            console.warn(message);
            return;
          default:
            return;
        }
      },
    },
  },
};

/**
 * Scopes you add here will be prompted for user consent during sign-in.
 * By default, MSAL.js will add OIDC scopes (openid, profile, email) to any login request.
 * For more information about OIDC scopes, visit: 
 * https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-permissions-and-consent#openid-connect-scopes
 */
export const loginRequest = {
  scopes: ["User.Read"],
};

/**
 * Add here the scopes to request when obtaining an access token for the API.
 */
export const apiRequest = {
  scopes: [import.meta.env.VITE_API_SCOPE || "api://your-api-id/.default"],
};
