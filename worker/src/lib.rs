use worker::*;

mod auth;
mod error;
mod models;
mod queue;

#[event(fetch)]
async fn main(req: Request, env: Env, _ctx: Context) -> Result<Response> {
    let router = Router::new();

    router
        // Health check (no auth required)
        .get_async("/health", |_, _ctx| async move {
            Response::from_json(&serde_json::json!({
                "status": "ok",
                "version": "1.0.0",
                "timestamp": chrono::Utc::now().timestamp_millis(),
            }))
        })
        // Authentication endpoints
        .post_async("/auth/login", |req, ctx| async move {
            auth::handle_login(req, ctx.env).await
        })
        .get_async("/auth/whoami", |req, ctx| async move {
            auth::handle_whoami(req, ctx.env).await
        })
        .post_async("/auth/logout", |req, ctx| async move {
            auth::handle_logout(req, ctx.env).await
        })
        .run(req, env)
        .await
}
