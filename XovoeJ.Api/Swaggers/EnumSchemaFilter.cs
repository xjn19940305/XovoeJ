
using Swashbuckle.AspNetCore.SwaggerGen;
using XovoeJ.Infrastructure;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
namespace XovoeJ.Api.Swaggers
{
    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                System.Enum.GetNames(context.Type)
                     .ToList()
                     .ForEach(name =>
                     {
                         var em = System.Enum.Parse(context.Type, name);
                         schema.Enum.Add(new OpenApiString(value: $"{Convert.ToInt64(System.Enum.Parse(context.Type, name))} = {((System.Enum)em).GetDescription()}"));
                     });
            }
        }
    }
}
