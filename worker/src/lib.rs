use worker::*;

mod auth;
mod error;
mod models;
mod queue;

#[event(fetch)]
async fn main(req: Request, env: Env, _ctx: Context) -> Result<Response> {
    let router = Router::new();

    router
        .get_async("/health", |_, _| async move {
            Response::from_json(&serde_json::json!({
                "status": "ok",
                "version": "1.0.0",
                "timestamp": chrono::Utc::now().timestamp_millis(),
            }))
        })
        .run(req, env)
        .await
}
