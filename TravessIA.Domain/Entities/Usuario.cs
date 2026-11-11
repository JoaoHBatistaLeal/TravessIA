using TravessIA.Domain.Exceptions;
using TravessIA.Domain.Helpers;

namespace TravessIA.Domain.Entities;

public class Usuario
{
    public const string RoleCidadao = "CIDADAO";
    public const string RoleGestor = "GESTOR";
    private const int MinimumPasswordLength = 8;

    public Guid Id { get; private set; }

    public Guid IdPerfil { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string SenhaHash { get; private set; } = string.Empty;

    public string Role { get; private set; } = RoleCidadao;

    public int AnoNascimento { get; private set; }

    public string ConsentimentoLgpd { get; private set; } = "S";

    public DateTime DtCadastro { get; private set; }

    public PerfilMobilidade? PerfilMobilidade { get; private set; }

    public List<Acionamento> Acionamentos { get; private set; } = [];

    private Usuario()
    {
    }

    public Usuario(
        Guid idPerfil,
        string nome,
        string email,
        string rawPassword,
        string role,
        int anoNascimento,
        string consentimentoLgpd,
        Guid? id = null)
    {
        Id = id.HasValue && id.Value != Guid.Empty ? id.Value : Guid.NewGuid();
        Update(idPerfil, nome, email, role, anoNascimento, consentimentoLgpd);
        ChangePassword(rawPassword);
        DtCadastro = DateTime.UtcNow;
    }

    public void Update(
        Guid idPerfil,
        string nome,
        string email,
        string role,
        int anoNascimento,
        string consentimentoLgpd)
    {
        if (idPerfil == Guid.Empty)
            throw new DomainException("Perfil de mobilidade invalido.");

        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome do usuario nao pode ser vazio.");

        var trimmedNome = nome.Trim();
        if (trimmedNome.Length > 120)
            throw new DomainException("Nome do usuario deve ter no maximo 120 caracteres.");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainException("E-mail invalido.");

        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (normalizedEmail.Length > 150)
            throw new DomainException("E-mail deve ter no maximo 150 caracteres.");

        var normalizedRole = string.IsNullOrWhiteSpace(role)
            ? RoleCidadao
            : role.Trim().ToUpperInvariant();

        if (normalizedRole != RoleCidadao && normalizedRole != RoleGestor)
            throw new DomainException("Role invalida. Valores permitidos: CIDADAO ou GESTOR.");

        var currentYear = DateTime.UtcNow.Year;
        if (anoNascimento < 1900 || anoNascimento > currentYear)
            throw new DomainException("Ano de nascimento invalido.");

        var normalizedConsent = string.IsNullOrWhiteSpace(consentimentoLgpd)
            ? string.Empty
            : consentimentoLgpd.Trim().ToUpperInvariant();

        if (normalizedConsent != "S" && normalizedConsent != "N")
            throw new DomainException("Consentimento LGPD deve ser S ou N.");

        IdPerfil = idPerfil;
        Nome = trimmedNome;
        Email = normalizedEmail;
        Role = normalizedRole;
        AnoNascimento = anoNascimento;
        ConsentimentoLgpd = normalizedConsent;
    }

    public void ChangePassword(string newRawPassword)
    {
        if (string.IsNullOrWhiteSpace(newRawPassword) || newRawPassword.Length < MinimumPasswordLength)
            throw new DomainException($"A senha deve ter pelo menos {MinimumPasswordLength} caracteres.");

        SenhaHash = HashHelper.Hash(newRawPassword);
    }

    public bool VerifyPassword(string rawPassword)
    {
        if (string.IsNullOrEmpty(rawPassword) || string.IsNullOrEmpty(SenhaHash))
            return false;

        return HashHelper.Verify(rawPassword, SenhaHash);
    }

    public void EnsureLgpdConsent()
    {
        if (ConsentimentoLgpd != "S")
            throw new DomainException("Acionamento negado: usuario nao concedeu consentimento LGPD obrigatorio (S).");
    }
}
