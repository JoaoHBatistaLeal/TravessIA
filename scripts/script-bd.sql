IF OBJECT_ID('dbo.TB_PERFIL_MOBILIDADE', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_PERFIL_MOBILIDADE (
        id_perfil UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_TB_PERFIL_MOBILIDADE_ID DEFAULT NEWID(),
        codigo VARCHAR(30) NOT NULL,
        descricao VARCHAR(150) NOT NULL,
        seg_extra_seco INT NOT NULL,
        seg_extra_chuva INT NOT NULL,
        CONSTRAINT PK_TB_PERFIL_MOBILIDADE PRIMARY KEY (id_perfil),
        CONSTRAINT UK_TB_PERFIL_MOBILIDADE_CODIGO UNIQUE (codigo)
    );
END;

IF OBJECT_ID('dbo.TB_USUARIO', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_USUARIO (
        id_usuario UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_TB_USUARIO_ID DEFAULT NEWID(),
        id_perfil UNIQUEIDENTIFIER NOT NULL,
        nome VARCHAR(120) NOT NULL,
        email VARCHAR(150) NOT NULL,
        senha_hash VARCHAR(255) NOT NULL,
        role VARCHAR(20) NOT NULL,
        ano_nascimento INT NOT NULL,
        consentimento_lgpd CHAR(1) NOT NULL,
        dt_cadastro DATETIME2 NOT NULL,
        CONSTRAINT PK_TB_USUARIO PRIMARY KEY (id_usuario),
        CONSTRAINT UK_TB_USUARIO_EMAIL UNIQUE (email),
        CONSTRAINT FK_TB_USUARIO_PERFIL FOREIGN KEY (id_perfil) REFERENCES dbo.TB_PERFIL_MOBILIDADE(id_perfil)
    );
END;

IF OBJECT_ID('dbo.TB_SEMAFORO', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_SEMAFORO (
        id_semaforo UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_TB_SEMAFORO_ID DEFAULT NEWID(),
        codigo_beacon VARCHAR(50) NOT NULL,
        endereco VARCHAR(200) NOT NULL,
        latitude DECIMAL(10,7) NOT NULL,
        longitude DECIMAL(10,7) NOT NULL,
        largura_via_m DECIMAL(5,2) NOT NULL,
        tempo_base_seg INT NOT NULL,
        status_operacional VARCHAR(30) NOT NULL,
        CONSTRAINT PK_TB_SEMAFORO PRIMARY KEY (id_semaforo),
        CONSTRAINT UK_TB_SEMAFORO_BEACON UNIQUE (codigo_beacon)
    );
END;

IF OBJECT_ID('dbo.TB_ACIONAMENTO', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_ACIONAMENTO (
        id_acionamento UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_TB_ACIONAMENTO_ID DEFAULT NEWID(),
        id_usuario UNIQUEIDENTIFIER NOT NULL,
        id_semaforo UNIQUEIDENTIFIER NOT NULL,
        chave_idempotencia VARCHAR(100) NOT NULL,
        dt_hora DATETIME2 NOT NULL,
        distancia_sensor_m DECIMAL(5,2) NOT NULL,
        condicao_clima VARCHAR(30) NOT NULL,
        intensidade_chuva_mm DECIMAL(6,2) NOT NULL,
        tempo_extra_seg INT NOT NULL,
        justificativa_ia NVARCHAR(MAX) NULL,
        CONSTRAINT PK_TB_ACIONAMENTO PRIMARY KEY (id_acionamento),
        CONSTRAINT UK_TB_ACIONAMENTO_IDEMPOTENCIA UNIQUE (chave_idempotencia),
        CONSTRAINT FK_TB_ACIONAMENTO_USUARIO FOREIGN KEY (id_usuario) REFERENCES dbo.TB_USUARIO(id_usuario),
        CONSTRAINT FK_TB_ACIONAMENTO_SEMAFORO FOREIGN KEY (id_semaforo) REFERENCES dbo.TB_SEMAFORO(id_semaforo)
    );
END;

IF OBJECT_ID('dbo.TB_AUDITORIA_SEMAFORO', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_AUDITORIA_SEMAFORO (
        id_auditoria UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_TB_AUDITORIA_SEMAFORO_ID DEFAULT NEWID(),
        id_semaforo UNIQUEIDENTIFIER NOT NULL,
        operacao VARCHAR(20) NOT NULL,
        campo_alterado VARCHAR(60) NOT NULL,
        valor_antigo VARCHAR(255) NULL,
        valor_novo VARCHAR(255) NULL,
        usuario_bd VARCHAR(100) NOT NULL,
        dt_operacao DATETIME2 NOT NULL,
        CONSTRAINT PK_TB_AUDITORIA_SEMAFORO PRIMARY KEY (id_auditoria)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TB_USUARIO_PERFIL' AND object_id = OBJECT_ID('dbo.TB_USUARIO'))
BEGIN
    CREATE INDEX IX_TB_USUARIO_PERFIL ON dbo.TB_USUARIO(id_perfil);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TB_ACIONAMENTO_USUARIO' AND object_id = OBJECT_ID('dbo.TB_ACIONAMENTO'))
BEGIN
    CREATE INDEX IX_TB_ACIONAMENTO_USUARIO ON dbo.TB_ACIONAMENTO(id_usuario);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TB_ACIONAMENTO_SEMAFORO' AND object_id = OBJECT_ID('dbo.TB_ACIONAMENTO'))
BEGIN
    CREATE INDEX IX_TB_ACIONAMENTO_SEMAFORO ON dbo.TB_ACIONAMENTO(id_semaforo);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TB_AUDITORIA_SEMAFORO_ID_SEMAFORO' AND object_id = OBJECT_ID('dbo.TB_AUDITORIA_SEMAFORO'))
BEGIN
    CREATE INDEX IX_TB_AUDITORIA_SEMAFORO_ID_SEMAFORO ON dbo.TB_AUDITORIA_SEMAFORO(id_semaforo);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TB_PERFIL_MOBILIDADE WHERE id_perfil = '11111111-1111-1111-1111-111111111101')
BEGIN
    INSERT INTO dbo.TB_PERFIL_MOBILIDADE (id_perfil, codigo, descricao, seg_extra_seco, seg_extra_chuva)
    VALUES ('11111111-1111-1111-1111-111111111101', 'PADRAO', 'Cidadao adulto sem restricoes de mobilidade', 0, 4);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TB_PERFIL_MOBILIDADE WHERE id_perfil = '11111111-1111-1111-1111-111111111102')
BEGIN
    INSERT INTO dbo.TB_PERFIL_MOBILIDADE (id_perfil, codigo, descricao, seg_extra_seco, seg_extra_chuva)
    VALUES ('11111111-1111-1111-1111-111111111102', 'IDOSO', 'Pessoa idosa acima de 60 anos com marcha reduzida', 6, 12);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TB_PERFIL_MOBILIDADE WHERE id_perfil = '11111111-1111-1111-1111-111111111103')
BEGIN
    INSERT INTO dbo.TB_PERFIL_MOBILIDADE (id_perfil, codigo, descricao, seg_extra_seco, seg_extra_chuva)
    VALUES ('11111111-1111-1111-1111-111111111103', 'CADEIRANTE', 'Usuario de cadeira de rodas manual ou motorizada', 8, 15);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TB_PERFIL_MOBILIDADE WHERE id_perfil = '11111111-1111-1111-1111-111111111104')
BEGIN
    INSERT INTO dbo.TB_PERFIL_MOBILIDADE (id_perfil, codigo, descricao, seg_extra_seco, seg_extra_chuva)
    VALUES ('11111111-1111-1111-1111-111111111104', 'DEFICIENCIA_VISUAL', 'Pessoa cega ou com baixa visao guiada por sinal sonoro', 7, 14);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TB_PERFIL_MOBILIDADE WHERE id_perfil = '11111111-1111-1111-1111-111111111105')
BEGIN
    INSERT INTO dbo.TB_PERFIL_MOBILIDADE (id_perfil, codigo, descricao, seg_extra_seco, seg_extra_chuva)
    VALUES ('11111111-1111-1111-1111-111111111105', 'MOBILIDADE_REDUZIDA', 'Gestante, pessoa com muletas ou mobilidade temporariamente reduzida', 5, 10);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TB_SEMAFORO WHERE id_semaforo = '22222222-2222-2222-2222-222222222201')
BEGIN
    INSERT INTO dbo.TB_SEMAFORO (id_semaforo, codigo_beacon, endereco, latitude, longitude, largura_via_m, tempo_base_seg, status_operacional)
    VALUES ('22222222-2222-2222-2222-222222222201', 'BEACON-SP-001', 'Av. Paulista, 1106 - Bela Vista, Sao Paulo - SP', -23.5641090, -46.6524090, 18.50, 15, 'ATIVO');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TB_SEMAFORO WHERE id_semaforo = '22222222-2222-2222-2222-222222222202')
BEGIN
    INSERT INTO dbo.TB_SEMAFORO (id_semaforo, codigo_beacon, endereco, latitude, longitude, largura_via_m, tempo_base_seg, status_operacional)
    VALUES ('22222222-2222-2222-2222-222222222202', 'BEACON-SP-002', 'Av. Brig. Faria Lima, 2000 - Pinheiros, Sao Paulo - SP', -23.5746120, -46.6891450, 22.00, 18, 'ATIVO');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.TB_SEMAFORO WHERE id_semaforo = '22222222-2222-2222-2222-222222222203')
BEGIN
    INSERT INTO dbo.TB_SEMAFORO (id_semaforo, codigo_beacon, endereco, latitude, longitude, largura_via_m, tempo_base_seg, status_operacional)
    VALUES ('22222222-2222-2222-2222-222222222203', 'BEACON-SP-003', 'Rua da Consolacao, 800 - Consolacao, Sao Paulo - SP', -23.5489210, -46.6501120, 16.00, 14, 'MODO_CHUVA');
END;
