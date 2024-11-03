using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;


public class CustomHeaderParameter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // 獲取當前控制器和動作的描述符
        // ApiDescription.ActionDescriptor
        var actionDescriptor = context.ApiDescription.ActionDescriptor;

        // 登入註冊方法不放置 Authorization 參數欄位
        if (actionDescriptor.RouteValues["action"] == "SignIn")
        {
            return; 
        }

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Authorization",
            In = ParameterLocation.Header,
            Description = "JWT Token",
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Pattern = "^Bearer .*"  // 要求值需以 "Bearer " 開頭
            }
        });
    }
}
