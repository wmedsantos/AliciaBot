using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AlicIA.Api.Swagger;

public class TimeSpanStringSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema == null || context.Type != typeof(string))
            return;

        var memberName = context.MemberInfo?.Name?.ToLowerInvariant();
        if (memberName is not ("starttime" or "endtime"))
            return;

        schema.Format = "time";
        schema.Example = new OpenApiString("08:00:00");
        schema.Description ??= "Time in HH:mm:ss format.";
    }
}
