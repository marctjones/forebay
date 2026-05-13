use worker::*;

mod auth;
mod error;
mod models;
mod queue;
mod storage;

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
        // Authentication endpoint
        .get_async("/auth/whoami", |req, ctx| async move {
            auth::handle_whoami(req, ctx.env).await
        })
        // Queue endpoints (all require auth via API key)
        .post_async("/queues/:queue/push", queue::handle_push)
        .post_async("/queues/:queue/pull", queue::handle_pull)
        .get_async("/queues/:queue/stats", queue::handle_stats)
        .delete_async("/queues/:queue", queue::handle_delete)
        .get_async("/queues", queue::handle_list)
        // Storage endpoints (all require auth via API key)
        .put_async("/store/*key", storage::handle_put)
        .get_async("/store/*key", storage::handle_get)
        .delete_async("/store/*key", storage::handle_delete_document)
        .get_async("/store", storage::handle_list_documents)
        .run(req, env)
        .await
}
