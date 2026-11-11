using System.ComponentModel;
using TravessIA.Domain.Entities;

namespace TravessIA.Application.DTOs.Auditorias;

[Description("Registro imutavel de auditoria de semaforo armazenado em TB_AUDITORIA_SEMAFORO.")]
public sealed record AuditoriaSemaforoResponse(
    [property: Description("Identificador unico da auditoria (id_auditoria).")]
    int IdAuditoria,

    [property: Description("Identificador do semaforo auditado (id_semaforo, sem FK).")]
    int IdSemaforo,

    [property: Description("Operacao realizada: INSERT, UPDATE ou DELETE.")]
    string Operacao,

    [property: Description("Nome do campo ou escopo alterado (campo_alterado).")]
    string CampoAlterado,

    [property: Description("Valor anterior do registro antes da alteracao (valor_antigo).")]
    string? ValorAntigo,

    [property: Description("Novo valor do registro apos a alteracao (valor_novo).")]
    string? ValorNovo,

    [property: Description("Identificacao do usuario de banco ou contexto de execucao (usuario_bd).")]
    string UsuarioBd,

    [property: Description("Data e hora UTC da operacao auditada (dt_operacao).")]
    DateTime DtOperacao)
{
    public static AuditoriaSemaforoResponse FromDomain(AuditoriaSemaforo entity) =>
        new(
            entity.Id,
            entity.IdSemaforo,
            entity.Operacao,
            entity.CampoAlterado,
            entity.ValorAntigo,
            entity.ValorNovo,
            entity.UsuarioBd,
            entity.DtOperacao);
}
