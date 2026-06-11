CREATE TABLE IF NOT EXISTS public.relatorio_templates (
    id uuid PRIMARY KEY,
    nome text NOT NULL,
    spec_json jsonb NOT NULL,
    template_html text NOT NULL DEFAULT '',
    criado_em timestamptz NOT NULL DEFAULT now(),
    atualizado_em timestamptz
);
