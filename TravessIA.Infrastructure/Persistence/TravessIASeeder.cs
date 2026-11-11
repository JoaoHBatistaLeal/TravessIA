using TravessIA.Domain.Entities;

namespace TravessIA.Infrastructure.Persistence;

public static class TravessIASeeder
{
    public static readonly Guid PerfilPadraoId = Guid.Parse("11111111-1111-1111-1111-111111111101");
    public static readonly Guid PerfilIdosoId = Guid.Parse("11111111-1111-1111-1111-111111111102");
    public static readonly Guid PerfilCadeiranteId = Guid.Parse("11111111-1111-1111-1111-111111111103");
    public static readonly Guid PerfilDeficienciaVisualId = Guid.Parse("11111111-1111-1111-1111-111111111104");
    public static readonly Guid PerfilMobilidadeReduzidaId = Guid.Parse("11111111-1111-1111-1111-111111111105");

    public static readonly Guid SemaforoSp001Id = Guid.Parse("22222222-2222-2222-2222-222222222201");
    public static readonly Guid SemaforoSp002Id = Guid.Parse("22222222-2222-2222-2222-222222222202");
    public static readonly Guid SemaforoSp003Id = Guid.Parse("22222222-2222-2222-2222-222222222203");

    public static readonly Guid UsuarioGestorId = Guid.Parse("33333333-3333-3333-3333-333333333301");
    public static readonly Guid UsuarioCidadaoId = Guid.Parse("33333333-3333-3333-3333-333333333302");

    public static void Seed(TravessIAContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.Database.EnsureCreated();

        if (!context.PerfisMobilidade.Any())
        {
            var perfis = new List<PerfilMobilidade>
            {
                new("PADRAO", "Cidadao adulto sem restricoes de mobilidade", 0, 4, PerfilPadraoId),
                new("IDOSO", "Pessoa idosa acima de 60 anos com marcha reduzida", 6, 12, PerfilIdosoId),
                new("CADEIRANTE", "Usuario de cadeira de rodas manual ou motorizada", 8, 15, PerfilCadeiranteId),
                new("DEFICIENCIA_VISUAL", "Pessoa cega ou com baixa visao guiada por sinal sonoro", 7, 14, PerfilDeficienciaVisualId),
                new("MOBILIDADE_REDUZIDA", "Gestante, pessoa com muletas ou mobilidade temporariamente reduzida", 5, 10, PerfilMobilidadeReduzidaId)
            };

            context.PerfisMobilidade.AddRange(perfis);
            context.SaveChanges();
        }

        if (!context.Semaforos.Any())
        {
            var semaforos = new List<Semaforo>
            {
                new("BEACON-SP-001", "Av. Paulista, 1106 - Bela Vista, Sao Paulo - SP", -23.5641090m, -46.6524090m, 18.50m, 15, Semaforo.StatusAtivo, SemaforoSp001Id),
                new("BEACON-SP-002", "Av. Brig. Faria Lima, 2000 - Pinheiros, Sao Paulo - SP", -23.5746120m, -46.6891450m, 22.00m, 18, Semaforo.StatusAtivo, SemaforoSp002Id),
                new("BEACON-SP-003", "Rua da Consolacao, 800 - Consolacao, Sao Paulo - SP", -23.5489210m, -46.6501120m, 16.00m, 14, Semaforo.StatusModoChuva, SemaforoSp003Id)
            };

            context.Semaforos.AddRange(semaforos);
            context.SaveChanges();

            if (!context.AuditoriasSemaforo.Any())
            {
                var auditorias = semaforos.Select(s => new AuditoriaSemaforo(
                    s.Id,
                    AuditoriaSemaforo.OperacaoInsert,
                    "SEED_INICIAL",
                    null,
                    $"{s.CodigoBeacon} | {s.StatusOperacional} | Base={s.TempoBaseSeg}s",
                    "travessia_seeder")).ToList();

                context.AuditoriasSemaforo.AddRange(auditorias);
                context.SaveChanges();
            }
        }

        if (!context.Usuarios.Any())
        {
            var perfilPadrao = context.PerfisMobilidade.First(p => p.Codigo == "PADRAO");
            var perfilIdoso = context.PerfisMobilidade.First(p => p.Codigo == "IDOSO");
            var demoAccess = Environment.GetEnvironmentVariable("TRAVESSIA_DEMO_ACCESS") ?? "TravessIA#2026";

            var usuarios = new List<Usuario>
            {
                new(
                    perfilPadrao.Id,
                    "Gestor CET Sao Paulo",
                    "gestor@travessia.sp.gov.br",
                    demoAccess,
                    Usuario.RoleGestor,
                    1985,
                    "S",
                    UsuarioGestorId),
                new(
                    perfilIdoso.Id,
                    "Cidadao Prioritario TravessIA",
                    "cidadao@travessia.sp.gov.br",
                    demoAccess,
                    Usuario.RoleCidadao,
                    1952,
                    "S",
                    UsuarioCidadaoId)
            };

            context.Usuarios.AddRange(usuarios);
            context.SaveChanges();
        }
    }
}
