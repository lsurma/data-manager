using MediatR;

namespace DataManager.Application.Contracts.Modules.Mjml;

public class RenderTemplateCommand : IRequest<RenderedTemplateDto>
{
    public string Html { get; set; } = "";
    public string Variables { get; set; } = "";
}
