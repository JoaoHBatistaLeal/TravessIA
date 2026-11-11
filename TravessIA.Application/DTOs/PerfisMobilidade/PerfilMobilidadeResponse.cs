using System.ComponentModel;
using TravessIA.Domain.Entities;

namespace TravessIA.Application.DTOs.PerfisMobilidade;

[Description("Representacao de um perfil de mobilidade cadastrado em TB_PERFIL_MOBILIDADE.")]
public sealed record PerfilMobilidadeResponse(
    [property: Description("Identificador unico do perfil de mobilidade (id_perfil).")]
    Guid IdPerfil,

    [property: Description("Codigo unico do perfil de mobilidade.")]
    string Codigo,

    [property: Description("Descricao do perfil de mobilidade.")]
    string Descricao,

    [property: Description("Segundos extras em tempo seco (seg_extra_seco).")]
    int SegExtraSeco,

    [property: Description("Segundos extras em tempo chuvoso (seg_extra_chuva).")]
    int SegExtraChuva)
{
    public static PerfilMobilidadeResponse FromDomain(PerfilMobilidade entity) =>
        new(
            entity.Id,
            entity.Codigo,
            entity.Descricao,
            entity.SegExtraSeco,
            entity.SegExtraChuva);
}
