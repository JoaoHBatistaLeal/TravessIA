using System.ComponentModel;
using TravessIA.Domain.Entities;

namespace TravessIA.Application.DTOs.PerfisMobilidade;

[Description("Payload para criacao ou atualizacao de um perfil de mobilidade em TB_PERFIL_MOBILIDADE.")]
public sealed record PerfilMobilidadeRequest(
    [property: Description("Codigo unico identificador do perfil (ex.: PADRAO, IDOSO, CADEIRANTE, DEFICIENCIA_VISUAL, MOBILIDADE_REDUZIDA).")]
    string Codigo,

    [property: Description("Descricao detalhada do perfil de mobilidade (maximo 150 caracteres).")]
    string Descricao,

    [property: Description("Segundos adicionais concedidos na travessia em condicao climatica seca.")]
    int SegExtraSeco,

    [property: Description("Segundos adicionais concedidos na travessia em condicao de chuva ou pista molhada.")]
    int SegExtraChuva)
{
    public PerfilMobilidade ToDomain() => new(Codigo, Descricao, SegExtraSeco, SegExtraChuva);
}
