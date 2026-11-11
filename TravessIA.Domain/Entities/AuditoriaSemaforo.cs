using TravessIA.Domain.Exceptions;

namespace TravessIA.Domain.Entities;

public class AuditoriaSemaforo
{
    public const string OperacaoInsert = "INSERT";
    public const string OperacaoUpdate = "UPDATE";
    public const string OperacaoDelete = "DELETE";

    public Guid Id { get; private set; }

    public Guid IdSemaforo { get; private set; }

    public string Operacao { get; private set; } = string.Empty;

    public string CampoAlterado { get; private set; } = string.Empty;

    public string? ValorAntigo { get; private set; }

    public string? ValorNovo { get; private set; }

    public string UsuarioBd { get; private set; } = string.Empty;

    public DateTime DtOperacao { get; private set; }

    private AuditoriaSemaforo()
    {
    }

    public AuditoriaSemaforo(
        Guid idSemaforo,
        string operacao,
        string campoAlterado,
        string? valorAntigo,
        string? valorNovo,
        string usuarioBd,
        Guid? id = null)
    {
        if (idSemaforo == Guid.Empty)
            throw new DomainException("Identificador do semaforo para auditoria e invalido.");

        if (string.IsNullOrWhiteSpace(operacao))
            throw new DomainException("Operacao de auditoria e obrigatoria.");

        var normalizedOperacao = operacao.Trim().ToUpperInvariant();
        if (normalizedOperacao != OperacaoInsert && normalizedOperacao != OperacaoUpdate && normalizedOperacao != OperacaoDelete)
            throw new DomainException("Operacao de auditoria invalida. Valores permitidos: INSERT, UPDATE, DELETE.");

        if (string.IsNullOrWhiteSpace(campoAlterado))
            throw new DomainException("Campo alterado na auditoria e obrigatorio.");

        Id = id.HasValue && id.Value != Guid.Empty ? id.Value : Guid.NewGuid();
        IdSemaforo = idSemaforo;
        Operacao = normalizedOperacao;
        CampoAlterado = Truncate(campoAlterado.Trim(), 60);
        ValorAntigo = valorAntigo is null ? null : Truncate(valorAntigo, 255);
        ValorNovo = valorNovo is null ? null : Truncate(valorNovo, 255);
        UsuarioBd = string.IsNullOrWhiteSpace(usuarioBd) ? "travessia_api" : Truncate(usuarioBd.Trim(), 100);
        DtOperacao = DateTime.UtcNow;
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
