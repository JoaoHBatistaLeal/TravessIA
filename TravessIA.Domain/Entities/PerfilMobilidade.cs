using TravessIA.Domain.Exceptions;

namespace TravessIA.Domain.Entities;

public class PerfilMobilidade
{
    public Guid Id { get; private set; }

    public string Codigo { get; private set; } = string.Empty;

    public string Descricao { get; private set; } = string.Empty;

    public int SegExtraSeco { get; private set; }

    public int SegExtraChuva { get; private set; }

    public List<Usuario> Usuarios { get; private set; } = [];

    private PerfilMobilidade()
    {
    }

    public PerfilMobilidade(
        string codigo,
        string descricao,
        int segExtraSeco,
        int segExtraChuva,
        Guid? id = null)
    {
        Id = id.HasValue && id.Value != Guid.Empty ? id.Value : Guid.NewGuid();
        Update(codigo, descricao, segExtraSeco, segExtraChuva);
    }

    public void Update(string codigo, string descricao, int segExtraSeco, int segExtraChuva)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new DomainException("Codigo do perfil de mobilidade e obrigatorio.");

        var normalizedCodigo = codigo.Trim().ToUpperInvariant();
        if (normalizedCodigo.Length > 30)
            throw new DomainException("Codigo do perfil deve ter no maximo 30 caracteres.");

        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("Descricao do perfil de mobilidade e obrigatoria.");

        var normalizedDescricao = descricao.Trim();
        if (normalizedDescricao.Length > 150)
            throw new DomainException("Descricao do perfil deve ter no maximo 150 caracteres.");

        if (segExtraSeco < 0)
            throw new DomainException("Segundos extras em tempo seco nao podem ser negativos.");

        if (segExtraChuva < 0)
            throw new DomainException("Segundos extras em chuva nao podem ser negativos.");

        if (segExtraChuva < segExtraSeco)
            throw new DomainException("O tempo extra em chuva nao pode ser menor que o tempo extra em tempo seco.");

        Codigo = normalizedCodigo;
        Descricao = normalizedDescricao;
        SegExtraSeco = segExtraSeco;
        SegExtraChuva = segExtraChuva;
    }
}
