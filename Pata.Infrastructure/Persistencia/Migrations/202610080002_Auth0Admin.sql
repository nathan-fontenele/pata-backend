ALTER TABLE usuarios
    ADD COLUMN IF NOT EXISTS "Auth0Sub" character varying(200),
    ADD COLUMN IF NOT EXISTS "Perfil" character varying(30) NOT NULL DEFAULT 'Usuario';

CREATE UNIQUE INDEX IF NOT EXISTS "IX_usuarios_tenant_id_Auth0Sub"
    ON usuarios (tenant_id, "Auth0Sub")
    WHERE "Auth0Sub" IS NOT NULL;
