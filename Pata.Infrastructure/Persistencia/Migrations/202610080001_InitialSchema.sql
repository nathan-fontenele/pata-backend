-- Idempotent baseline upgrade for existing EnsureCreated databases and fresh installs.
-- Add tenant columns to legacy tables without assigning existing records to a clinic.
-- NULL legacy rows are intentionally excluded by the application's tenant query filters.
DO $migration$
BEGIN
    IF to_regclass('tutores') IS NOT NULL THEN
        ALTER TABLE tutores ADD COLUMN IF NOT EXISTS tenant_id uuid;
        CREATE UNIQUE INDEX IF NOT EXISTS "IX_tutores_Id_tenant_id" ON tutores ("Id", tenant_id);
    END IF;
    IF to_regclass('veterinarios') IS NOT NULL THEN
        ALTER TABLE veterinarios ADD COLUMN IF NOT EXISTS tenant_id uuid;
        CREATE UNIQUE INDEX IF NOT EXISTS "IX_veterinarios_Id_tenant_id" ON veterinarios ("Id", tenant_id);
    END IF;
    IF to_regclass('animais') IS NOT NULL THEN
        ALTER TABLE animais ADD COLUMN IF NOT EXISTS tenant_id uuid;
        CREATE UNIQUE INDEX IF NOT EXISTS "IX_animais_Id_tenant_id" ON animais ("Id", tenant_id);
    END IF;
    IF to_regclass('consultas') IS NOT NULL THEN
        ALTER TABLE consultas ADD COLUMN IF NOT EXISTS tenant_id uuid;
        CREATE UNIQUE INDEX IF NOT EXISTS "IX_consultas_Id_tenant_id" ON consultas ("Id", tenant_id);
    END IF;
END
$migration$;

CREATE TABLE IF NOT EXISTS auditorias (
    "Id" uuid NOT NULL,
    "EntidadeId" uuid NOT NULL,
    "EntidadeTipo" character varying(150) NOT NULL,
    "Acao" character varying(30) NOT NULL,
    "CriadoEm" timestamp with time zone NOT NULL,
    "CriadoPor" character varying(150) NOT NULL,
    "EditadoEm" timestamp with time zone,
    "EditadoPor" character varying(150),
    "ExcluidoEm" timestamp with time zone,
    "ExcluidoPor" character varying(150),
    "EntidadeAntes" jsonb,
    "EntidadeDepois" jsonb,
    tenant_id uuid NOT NULL,
    CONSTRAINT "PK_auditorias" PRIMARY KEY ("Id")
);


CREATE TABLE IF NOT EXISTS organizacoes (
    "Id" uuid NOT NULL,
    "Nome" character varying(150) NOT NULL,
    "Slug" character varying(80) NOT NULL,
    "Status" character varying(30) NOT NULL,
    "Cnpj" character varying(14) NOT NULL,
    "Email" character varying(254) NOT NULL,
    "LogoUrl" character varying(2048),
    tenant_id uuid NOT NULL,
    CONSTRAINT "PK_organizacoes" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_organizacoes_Id_tenant_id" UNIQUE ("Id", tenant_id)
);


CREATE TABLE IF NOT EXISTS planos (
    "Id" uuid NOT NULL,
    "Codigo" character varying(50) NOT NULL,
    "Nome" character varying(120) NOT NULL,
    "Descricao" character varying(2000),
    "Status" character varying(30) NOT NULL,
    CONSTRAINT "PK_planos" PRIMARY KEY ("Id")
);


CREATE TABLE IF NOT EXISTS tutores (
    "Id" uuid NOT NULL,
    "Nome" character varying(150) NOT NULL,
    "Cpf" character varying(11) NOT NULL,
    "Email" character varying(254) NOT NULL,
    "Telefone" character varying(11) NOT NULL,
    tenant_id uuid NOT NULL,
    "CriadoEm" timestamp with time zone NOT NULL,
    "CriadoPor" character varying(150) NOT NULL,
    "AtualizadoEm" timestamp with time zone,
    "AtualizadoPor" character varying(150),
    "Excluido" boolean NOT NULL,
    "ExcluidoEm" timestamp with time zone,
    "ExcluidoPor" character varying(150),
    CONSTRAINT "PK_tutores" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_tutores_Id_tenant_id" UNIQUE ("Id", tenant_id)
);


CREATE TABLE IF NOT EXISTS veterinarios (
    "Id" uuid NOT NULL,
    "Nome" character varying(100) NOT NULL,
    "Email" character varying(254) NOT NULL,
    "Telefone" character varying(11) NOT NULL,
    "Crmv" character varying(9) NOT NULL,
    "Especialidade" character varying(100) NOT NULL,
    tenant_id uuid NOT NULL,
    "CriadoEm" timestamp with time zone NOT NULL,
    "CriadoPor" character varying(150) NOT NULL,
    "AtualizadoEm" timestamp with time zone,
    "AtualizadoPor" character varying(150),
    "Excluido" boolean NOT NULL,
    "ExcluidoEm" timestamp with time zone,
    "ExcluidoPor" character varying(150),
    CONSTRAINT "PK_veterinarios" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_veterinarios_Id_tenant_id" UNIQUE ("Id", tenant_id)
);


CREATE TABLE IF NOT EXISTS equipes (
    "Id" uuid NOT NULL,
    "OrganizacaoId" uuid NOT NULL,
    "Nome" character varying(120) NOT NULL,
    "Descricao" character varying(500),
    "Status" character varying(30) NOT NULL,
    tenant_id uuid NOT NULL,
    CONSTRAINT "PK_equipes" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_equipes_Id_tenant_id" UNIQUE ("Id", tenant_id),
    CONSTRAINT "FK_equipes_organizacoes_OrganizacaoId_tenant_id" FOREIGN KEY ("OrganizacaoId", tenant_id) REFERENCES organizacoes ("Id", tenant_id) ON DELETE RESTRICT
);


CREATE TABLE IF NOT EXISTS precos_plano (
    "Id" uuid NOT NULL,
    "PlanoId" uuid NOT NULL,
    "Codigo" character varying(50) NOT NULL,
    "Valor" numeric(18,2) NOT NULL,
    "Moeda" character varying(3) NOT NULL,
    "DiasTeste" integer NOT NULL,
    "IntervaloUnidade" character varying(20) NOT NULL,
    "IntervaloQuantidade" integer NOT NULL,
    "Status" character varying(30) NOT NULL,
    CONSTRAINT "PK_precos_plano" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_precos_plano_planos_PlanoId" FOREIGN KEY ("PlanoId") REFERENCES planos ("Id") ON DELETE RESTRICT
);


CREATE TABLE IF NOT EXISTS acessos_portal_tutor (
    "Id" uuid NOT NULL,
    "TutorId" uuid NOT NULL,
    "TokenHash" character varying(64) NOT NULL,
    "CriadoEm" timestamp with time zone NOT NULL,
    "RevogadoEm" timestamp with time zone,
    tenant_id uuid NOT NULL,
    CONSTRAINT "PK_acessos_portal_tutor" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_acessos_portal_tutor_tutores_TutorId_tenant_id" FOREIGN KEY ("TutorId", tenant_id) REFERENCES tutores ("Id", tenant_id) ON DELETE CASCADE
);


CREATE TABLE IF NOT EXISTS animais (
    "Id" uuid NOT NULL,
    "TutorId" uuid NOT NULL,
    tenant_id uuid NOT NULL,
    "Nome" character varying(100) NOT NULL,
    "Especie" character varying(30) NOT NULL,
    "Raca" character varying(100) NOT NULL,
    "DataNascimento" date NOT NULL,
    CONSTRAINT "PK_animais" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_animais_Id_tenant_id" UNIQUE ("Id", tenant_id),
    CONSTRAINT "FK_animais_tutores_TutorId_tenant_id" FOREIGN KEY ("TutorId", tenant_id) REFERENCES tutores ("Id", tenant_id) ON DELETE CASCADE
);


CREATE TABLE IF NOT EXISTS usuarios (
    "Id" uuid NOT NULL,
    "EquipeId" uuid NOT NULL,
    "Nome" character varying(100) NOT NULL,
    "Sobrenome" character varying(100) NOT NULL,
    "Email" character varying(254) NOT NULL,
    "Telefone" character varying(30),
    "Cargo" character varying(100),
    "UltimoAcessoEm" timestamp with time zone,
    "Status" character varying(30) NOT NULL,
    tenant_id uuid NOT NULL,
    CONSTRAINT "PK_usuarios" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_usuarios_Id_tenant_id" UNIQUE ("Id", tenant_id),
    CONSTRAINT "FK_usuarios_equipes_EquipeId_tenant_id" FOREIGN KEY ("EquipeId", tenant_id) REFERENCES equipes ("Id", tenant_id) ON DELETE RESTRICT
);


CREATE TABLE IF NOT EXISTS assinaturas (
    "Id" uuid NOT NULL,
    "OrganizacaoId" uuid NOT NULL,
    "PrecoPlanoId" uuid NOT NULL,
    "Status" character varying(30) NOT NULL,
    "InicioEm" timestamp with time zone NOT NULL,
    "PeriodoAtualInicio" timestamp with time zone,
    "PeriodoAtualFim" timestamp with time zone,
    "TesteInicio" timestamp with time zone,
    "TesteFim" timestamp with time zone,
    "CancelarEm" timestamp with time zone,
    "CancelamentoSolicitadoEm" timestamp with time zone,
    "EncerradoEm" timestamp with time zone,
    tenant_id uuid NOT NULL,
    CONSTRAINT "PK_assinaturas" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_assinaturas_Id_tenant_id" UNIQUE ("Id", tenant_id),
    CONSTRAINT "FK_assinaturas_organizacoes_OrganizacaoId_tenant_id" FOREIGN KEY ("OrganizacaoId", tenant_id) REFERENCES organizacoes ("Id", tenant_id) ON DELETE RESTRICT,
    CONSTRAINT "FK_assinaturas_precos_plano_PrecoPlanoId" FOREIGN KEY ("PrecoPlanoId") REFERENCES precos_plano ("Id") ON DELETE RESTRICT
);


CREATE TABLE IF NOT EXISTS consultas (
    "Id" uuid NOT NULL,
    "AnimalId" uuid NOT NULL,
    "VeterinarioId" uuid NOT NULL,
    "TutorId" uuid NOT NULL,
    "DataHora" timestamp with time zone NOT NULL,
    "ConfirmadaEm" timestamp with time zone,
    "Status" character varying(30) NOT NULL,
    tenant_id uuid NOT NULL,
    "CriadoEm" timestamp with time zone NOT NULL,
    "CriadoPor" character varying(150) NOT NULL,
    "AtualizadoEm" timestamp with time zone,
    "AtualizadoPor" character varying(150),
    "Excluido" boolean NOT NULL,
    "ExcluidoEm" timestamp with time zone,
    "ExcluidoPor" character varying(150),
    CONSTRAINT "PK_consultas" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_consultas_Id_tenant_id" UNIQUE ("Id", tenant_id),
    CONSTRAINT "FK_consultas_animais_AnimalId_tenant_id" FOREIGN KEY ("AnimalId", tenant_id) REFERENCES animais ("Id", tenant_id) ON DELETE RESTRICT,
    CONSTRAINT "FK_consultas_tutores_TutorId_tenant_id" FOREIGN KEY ("TutorId", tenant_id) REFERENCES tutores ("Id", tenant_id) ON DELETE RESTRICT,
    CONSTRAINT "FK_consultas_veterinarios_VeterinarioId_tenant_id" FOREIGN KEY ("VeterinarioId", tenant_id) REFERENCES veterinarios ("Id", tenant_id) ON DELETE RESTRICT
);


CREATE TABLE IF NOT EXISTS convites_usuario (
    "Id" uuid NOT NULL,
    "UsuarioId" uuid NOT NULL,
    "TokenHash" character varying(256) NOT NULL,
    "CriadoEm" timestamp with time zone NOT NULL,
    "ExpiraEm" timestamp with time zone NOT NULL,
    "AceitoEm" timestamp with time zone,
    "RevogadoEm" timestamp with time zone,
    "ConvidadoPorUsuarioId" uuid,
    tenant_id uuid NOT NULL,
    CONSTRAINT "PK_convites_usuario" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_convites_usuario_usuarios_ConvidadoPorUsuarioId_tenant_id" FOREIGN KEY ("ConvidadoPorUsuarioId", tenant_id) REFERENCES usuarios ("Id", tenant_id) ON DELETE RESTRICT,
    CONSTRAINT "FK_convites_usuario_usuarios_UsuarioId_tenant_id" FOREIGN KEY ("UsuarioId", tenant_id) REFERENCES usuarios ("Id", tenant_id) ON DELETE CASCADE
);


CREATE TABLE IF NOT EXISTS faturas (
    "Id" uuid NOT NULL,
    "AssinaturaId" uuid NOT NULL,
    "Numero" character varying(80) NOT NULL,
    "Status" character varying(30) NOT NULL,
    "Moeda" character varying(3) NOT NULL,
    "Subtotal" numeric(18,2) NOT NULL,
    "DescontoTotal" numeric(18,2) NOT NULL,
    "Total" numeric(18,2) NOT NULL,
    "EmitidaEm" timestamp with time zone NOT NULL,
    "VencimentoEm" timestamp with time zone NOT NULL,
    "QuitadaEm" timestamp with time zone,
    tenant_id uuid NOT NULL,
    CONSTRAINT "PK_faturas" PRIMARY KEY ("Id"),
    CONSTRAINT "AK_faturas_Id_tenant_id" UNIQUE ("Id", tenant_id),
    CONSTRAINT "FK_faturas_assinaturas_AssinaturaId_tenant_id" FOREIGN KEY ("AssinaturaId", tenant_id) REFERENCES assinaturas ("Id", tenant_id) ON DELETE RESTRICT
);


CREATE TABLE IF NOT EXISTS prontuarios (
    "Id" uuid NOT NULL,
    "ConsultaId" uuid NOT NULL,
    "Diagnostico" character varying(2000),
    "Prescricao" character varying(4000),
    "DataRegistro" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_prontuarios" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_prontuarios_consultas_ConsultaId" FOREIGN KEY ("ConsultaId") REFERENCES consultas ("Id") ON DELETE CASCADE
);


CREATE TABLE IF NOT EXISTS itens_fatura (
    "Id" uuid NOT NULL,
    "FaturaId" uuid NOT NULL,
    "PrecoPlanoId" uuid,
    "Descricao" character varying(500) NOT NULL,
    "Quantidade" numeric(18,4) NOT NULL,
    "ValorUnitario" numeric(18,2) NOT NULL,
    "Desconto" numeric(18,2) NOT NULL,
    "Total" numeric(18,2) NOT NULL,
    "PeriodoInicio" timestamp with time zone,
    "PeriodoFim" timestamp with time zone,
    tenant_id uuid NOT NULL,
    CONSTRAINT "PK_itens_fatura" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_itens_fatura_faturas_FaturaId_tenant_id" FOREIGN KEY ("FaturaId", tenant_id) REFERENCES faturas ("Id", tenant_id) ON DELETE CASCADE,
    CONSTRAINT "FK_itens_fatura_precos_plano_PrecoPlanoId" FOREIGN KEY ("PrecoPlanoId") REFERENCES precos_plano ("Id") ON DELETE RESTRICT
);


CREATE TABLE IF NOT EXISTS pagamentos (
    "Id" uuid NOT NULL,
    "FaturaId" uuid NOT NULL,
    "Status" character varying(30) NOT NULL,
    "Valor" numeric(18,2) NOT NULL,
    "Moeda" character varying(3) NOT NULL,
    "Metodo" character varying(50) NOT NULL,
    "Provedor" character varying(80) NOT NULL,
    "ReferenciaExterna" character varying(200),
    "ChaveIdempotencia" character varying(120) NOT NULL,
    "ConfirmadaEm" timestamp with time zone,
    "FalhouEm" timestamp with time zone,
    "CodigoFalha" character varying(100),
    "MensagemFalha" character varying(1000),
    tenant_id uuid NOT NULL,
    CONSTRAINT "PK_pagamentos" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_pagamentos_faturas_FaturaId_tenant_id" FOREIGN KEY ("FaturaId", tenant_id) REFERENCES faturas ("Id", tenant_id) ON DELETE RESTRICT
);


CREATE TABLE IF NOT EXISTS sintomas (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Descricao" character varying(500) NOT NULL,
    "ProntuarioId" uuid NOT NULL,
    CONSTRAINT "PK_sintomas" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_sintomas_prontuarios_ProntuarioId" FOREIGN KEY ("ProntuarioId") REFERENCES prontuarios ("Id") ON DELETE CASCADE
);


CREATE UNIQUE INDEX IF NOT EXISTS "IX_acessos_portal_tutor_tenant_id_TutorId" ON acessos_portal_tutor (tenant_id, "TutorId");


CREATE UNIQUE INDEX IF NOT EXISTS "IX_acessos_portal_tutor_TokenHash" ON acessos_portal_tutor ("TokenHash");


CREATE INDEX IF NOT EXISTS "IX_acessos_portal_tutor_TutorId_tenant_id" ON acessos_portal_tutor ("TutorId", tenant_id);


CREATE INDEX IF NOT EXISTS "IX_animais_TutorId_tenant_id" ON animais ("TutorId", tenant_id);


CREATE INDEX IF NOT EXISTS "IX_assinaturas_OrganizacaoId_tenant_id" ON assinaturas ("OrganizacaoId", tenant_id);


CREATE INDEX IF NOT EXISTS "IX_assinaturas_PrecoPlanoId" ON assinaturas ("PrecoPlanoId");


CREATE INDEX IF NOT EXISTS "IX_assinaturas_tenant_id_Status" ON assinaturas (tenant_id, "Status");


CREATE INDEX IF NOT EXISTS "IX_auditorias_tenant_id_EntidadeTipo_EntidadeId_CriadoEm" ON auditorias (tenant_id, "EntidadeTipo", "EntidadeId", "CriadoEm");


CREATE INDEX IF NOT EXISTS "IX_consultas_AnimalId_tenant_id" ON consultas ("AnimalId", tenant_id);


CREATE INDEX IF NOT EXISTS "IX_consultas_TutorId_tenant_id" ON consultas ("TutorId", tenant_id);


CREATE INDEX IF NOT EXISTS "IX_consultas_VeterinarioId_DataHora" ON consultas ("VeterinarioId", "DataHora");


CREATE INDEX IF NOT EXISTS "IX_consultas_VeterinarioId_tenant_id" ON consultas ("VeterinarioId", tenant_id);


CREATE INDEX IF NOT EXISTS "IX_convites_usuario_ConvidadoPorUsuarioId_tenant_id" ON convites_usuario ("ConvidadoPorUsuarioId", tenant_id);


CREATE UNIQUE INDEX IF NOT EXISTS "IX_convites_usuario_TokenHash" ON convites_usuario ("TokenHash");


CREATE INDEX IF NOT EXISTS "IX_convites_usuario_UsuarioId_tenant_id" ON convites_usuario ("UsuarioId", tenant_id);


CREATE INDEX IF NOT EXISTS "IX_equipes_OrganizacaoId_tenant_id" ON equipes ("OrganizacaoId", tenant_id);


CREATE INDEX IF NOT EXISTS "IX_faturas_AssinaturaId_tenant_id" ON faturas ("AssinaturaId", tenant_id);


CREATE UNIQUE INDEX IF NOT EXISTS "IX_faturas_tenant_id_Numero" ON faturas (tenant_id, "Numero");


CREATE INDEX IF NOT EXISTS "IX_itens_fatura_FaturaId_tenant_id" ON itens_fatura ("FaturaId", tenant_id);


CREATE INDEX IF NOT EXISTS "IX_itens_fatura_PrecoPlanoId" ON itens_fatura ("PrecoPlanoId");


CREATE UNIQUE INDEX IF NOT EXISTS "IX_organizacoes_Slug" ON organizacoes ("Slug");


CREATE INDEX IF NOT EXISTS "IX_pagamentos_FaturaId_tenant_id" ON pagamentos ("FaturaId", tenant_id);


CREATE UNIQUE INDEX IF NOT EXISTS "IX_pagamentos_tenant_id_ChaveIdempotencia" ON pagamentos (tenant_id, "ChaveIdempotencia");


CREATE UNIQUE INDEX IF NOT EXISTS "IX_planos_Codigo" ON planos ("Codigo");


CREATE UNIQUE INDEX IF NOT EXISTS "IX_precos_plano_Codigo" ON precos_plano ("Codigo");


CREATE INDEX IF NOT EXISTS "IX_precos_plano_PlanoId" ON precos_plano ("PlanoId");


CREATE UNIQUE INDEX IF NOT EXISTS "IX_prontuarios_ConsultaId" ON prontuarios ("ConsultaId");


CREATE INDEX IF NOT EXISTS "IX_sintomas_ProntuarioId" ON sintomas ("ProntuarioId");


CREATE UNIQUE INDEX IF NOT EXISTS "IX_tutores_tenant_id_Cpf" ON tutores (tenant_id, "Cpf");


CREATE INDEX IF NOT EXISTS "IX_usuarios_EquipeId_tenant_id" ON usuarios ("EquipeId", tenant_id);


CREATE UNIQUE INDEX IF NOT EXISTS "IX_usuarios_tenant_id_Email" ON usuarios (tenant_id, "Email");


CREATE UNIQUE INDEX IF NOT EXISTS "IX_veterinarios_tenant_id_Crmv" ON veterinarios (tenant_id, "Crmv");



-- Existing records remain tenantless until an explicit clinic mapping is supplied.
DO $migration$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_tutores_tenant_id_not_null' AND conrelid = to_regclass('tutores')) THEN
        ALTER TABLE tutores ADD CONSTRAINT "CK_tutores_tenant_id_not_null" CHECK (tenant_id IS NOT NULL) NOT VALID;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_veterinarios_tenant_id_not_null' AND conrelid = to_regclass('veterinarios')) THEN
        ALTER TABLE veterinarios ADD CONSTRAINT "CK_veterinarios_tenant_id_not_null" CHECK (tenant_id IS NOT NULL) NOT VALID;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_animais_tenant_id_not_null' AND conrelid = to_regclass('animais')) THEN
        ALTER TABLE animais ADD CONSTRAINT "CK_animais_tenant_id_not_null" CHECK (tenant_id IS NOT NULL) NOT VALID;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_consultas_tenant_id_not_null' AND conrelid = to_regclass('consultas')) THEN
        ALTER TABLE consultas ADD CONSTRAINT "CK_consultas_tenant_id_not_null" CHECK (tenant_id IS NOT NULL) NOT VALID;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_animais_tutores_TutorId_tenant_id' AND conrelid = to_regclass('animais')) THEN
        ALTER TABLE animais ADD CONSTRAINT "FK_animais_tutores_TutorId_tenant_id" FOREIGN KEY ("TutorId", tenant_id) REFERENCES tutores ("Id", tenant_id) ON DELETE CASCADE NOT VALID;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_consultas_animais_AnimalId_tenant_id' AND conrelid = to_regclass('consultas')) THEN
        ALTER TABLE consultas ADD CONSTRAINT "FK_consultas_animais_AnimalId_tenant_id" FOREIGN KEY ("AnimalId", tenant_id) REFERENCES animais ("Id", tenant_id) ON DELETE RESTRICT NOT VALID;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_consultas_tutores_TutorId_tenant_id' AND conrelid = to_regclass('consultas')) THEN
        ALTER TABLE consultas ADD CONSTRAINT "FK_consultas_tutores_TutorId_tenant_id" FOREIGN KEY ("TutorId", tenant_id) REFERENCES tutores ("Id", tenant_id) ON DELETE RESTRICT NOT VALID;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_consultas_veterinarios_VeterinarioId_tenant_id' AND conrelid = to_regclass('consultas')) THEN
        ALTER TABLE consultas ADD CONSTRAINT "FK_consultas_veterinarios_VeterinarioId_tenant_id" FOREIGN KEY ("VeterinarioId", tenant_id) REFERENCES veterinarios ("Id", tenant_id) ON DELETE RESTRICT NOT VALID;
    END IF;
END
$migration$;
