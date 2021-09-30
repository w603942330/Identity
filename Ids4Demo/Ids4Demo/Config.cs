using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ids4Demo
{

    //1. OAuth 2.0是有关如何颁发访问令牌的规范，提供Access Token用于访问服务接口的
    //2. OpenID Connect是有关如何发行ID令牌的规范，提供Id Token用于用户身份标识（非敏感信息），Id Token是基于JWT格式
    //3. IdentityServer4服务中心默认提供接口/connect/token获取access token
    //4. IdentityServer4新版本新增ApiScope配置保护API资源，并使用ApiScope结合策略授权完成了一个简单的权限控制

    
    public static class Config
    {
        //下载ids4的依赖：install-package IdentityServer4  -version 2.1.1
        // scopes define the resources in your system
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("api1", "第一个api接口")//api1为对应认证的API名：apiName
                {
                    //!!!重要
                    Scopes = { "scope1"},
                    UserClaims={JwtClaimTypes.Role},  //添加Cliam 角色类型
                    ApiSecrets={new Secret("apipwd".Sha256())}
                },
               
            };
        }

        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>
            {
                new ApiScope("scope1"),
            };
        }

        public static IEnumerable<TestUser> Users()
        {
            return new[]
            {
               

            new TestUser
                {


                    SubjectId = "1",
                    Username = "mail@qq.com",
                    Password = "password"
                  
                }
            };
        }

        // clients want to access resources (aka scopes)
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                 new Client
                {
                     
                     //密码模式：直接通过用户账号、密码等配置，直接请求获取token
                     //客户端模式：直接通过用户账号、密码等配置，直接请求获取token，再将token用于访问被保护的API
                     #region 同时支持密码模式和客户端两种模式访问  
                     ClientId = "appPassword",
                     ClientSecrets = { new Secret("secret".Sha256()) },
                     AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,//同时支持password和client_credentials两种模式访问

                     AllowedScopes = new List<string>
                     {
                         IdentityServerConstants.StandardScopes.OpenId,
                         IdentityServerConstants.StandardScopes.Profile,
                         "scope1",
                     }
                     #endregion

                   
                 },
                 new Client
                 { 
                     //根据授权码模式获取的token，同样可以用于访问受保护的API
                     #region 授权码模式

                        ClientId = "app",
                        ClientName = "code Auth",
                        ClientSecrets = { new Secret("secret".Sha256()) },
                        AllowedGrantTypes = GrantTypes.Code,

                        RedirectUris ={
                            "http://localhost:5002/signin-oidc", //跳转登录到的客户端MVC应用的地址
                        },
                        // RedirectUris = {"http://localhost:5002/auth.html" }, //跳转登出到的客户端MVC应用的地址
                        PostLogoutRedirectUris ={
                            "http://localhost:5002/signout-callback-oidc",
                        },
                        AllowedScopes = {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            "scope1"
                        },
                        //允许将token通过浏览器传递
                        AllowAccessTokensViaBrowser=true,
                        // 是否需要同意授权 （默认是false）
                        RequireConsent=true
                    #endregion
                 },

                 new Client()
                 {
                     #region 隐藏模式,跳转认证中心地址登录，成功后重定向到自己的应用，同时返回token
                     //客户端Id
                     ClientId="apiClientImpl",
                     ClientName="ApiClient for Implicit",
                     //客户端授权类型，Implicit:隐藏模式
                     AllowedGrantTypes=GrantTypes.Implicit,
                     //允许登录后重定向的地址列表，可以有多个
                     RedirectUris = {"https://localhost:5002/auth.html" },
                     //允许访问的资源
                     AllowedScopes={
                        "secretapi"
                     },
                     //允许将token通过浏览器传递
                     AllowAccessTokensViaBrowser=true
                    #endregion
                 }
            };
        }

    }
}
