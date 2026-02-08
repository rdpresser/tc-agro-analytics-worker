# TC Agro Analytics - Makefile
# Simplifies Docker Compose commands

.PHONY: help up down logs ps restart clean rebuild db-migrate db-seed

# Default target
.DEFAULT_GOAL := help

# Colors for output
BLUE := \033[0;34m
GREEN := \033[0;32m
YELLOW := \033[1;33m
RED := \033[0;31m
NC := \033[0m # No Color

help: ## Show this help message
	@echo "$(BLUE)TC Agro Analytics - Docker Infrastructure$(NC)"
	@echo ""
	@echo "$(GREEN)Available commands:$(NC)"
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  $(YELLOW)%-15s$(NC) %s\n", $$1, $$2}'

up: ## Start all services
	@echo "$(GREEN)🚀 Starting services...$(NC)"
	@docker-compose up -d
	@echo "$(GREEN)✅ Services started!$(NC)"
	@make ps

up-minimal: ## Start only essential services (PostgreSQL & RabbitMQ)
	@echo "$(GREEN)🚀 Starting minimal services...$(NC)"
	@docker-compose -f docker-compose.minimal.yml up -d
	@echo "$(GREEN)✅ Minimal services started!$(NC)"

down: ## Stop all services
	@echo "$(YELLOW)🛑 Stopping services...$(NC)"
	@docker-compose down
	@echo "$(GREEN)✅ Services stopped!$(NC)"

down-volumes: ## Stop services and remove volumes (⚠️  data will be lost)
	@echo "$(RED)⚠️  WARNING: This will delete all data!$(NC)"
	@read -p "Are you sure? [y/N] " -n 1 -r; \
	if [[ $$REPLY =~ ^[Yy]$$ ]]; then \
		echo ""; \
		docker-compose down -v; \
		echo "$(GREEN)✅ Services stopped and volumes removed!$(NC)"; \
	else \
		echo ""; \
		echo "$(YELLOW)Cancelled.$(NC)"; \
	fi

logs: ## Show logs from all services
	@docker-compose logs -f

logs-postgres: ## Show PostgreSQL logs
	@docker-compose logs -f tc-agro-postgres

logs-rabbitmq: ## Show RabbitMQ logs
	@docker-compose logs -f tc-agro-rabbitmq

logs-redis: ## Show Redis logs
	@docker-compose logs -f tc-agro-redis

ps: ## Show running services
	@echo "$(BLUE)📊 Service Status:$(NC)"
	@docker-compose ps
	@echo ""
	@echo "$(BLUE)🔗 Service URLs:$(NC)"
	@echo "  PostgreSQL:         localhost:5432"
	@echo "  RabbitMQ AMQP:      localhost:5672"
	@echo "  RabbitMQ Management: http://localhost:15672"
	@echo "  Redis:              localhost:6379"
	@echo "  Grafana:            http://localhost:3000"
	@echo "  pgAdmin:            http://localhost:5050"

restart: ## Restart all services
	@echo "$(YELLOW)🔄 Restarting services...$(NC)"
	@docker-compose restart
	@echo "$(GREEN)✅ Services restarted!$(NC)"

clean: ## Remove stopped containers and dangling images
	@echo "$(YELLOW)🧹 Cleaning up...$(NC)"
	@docker-compose down --remove-orphans
	@docker system prune -f
	@echo "$(GREEN)✅ Cleanup complete!$(NC)"

rebuild: ## Rebuild and restart all services
	@echo "$(YELLOW)🔨 Rebuilding services...$(NC)"
	@docker-compose down
	@docker-compose up -d --build
	@echo "$(GREEN)✅ Services rebuilt!$(NC)"

pull: ## Pull latest images
	@echo "$(BLUE)📥 Pulling latest images...$(NC)"
	@docker-compose pull
	@echo "$(GREEN)✅ Images updated!$(NC)"

db-shell: ## Open PostgreSQL shell
	@echo "$(BLUE)🐘 Connecting to PostgreSQL...$(NC)"
	@docker exec -it tc-agro-postgres psql -U postgres -d tc_agro_analytics

db-backup: ## Backup PostgreSQL database
	@echo "$(BLUE)💾 Backing up database...$(NC)"
	@docker exec tc-agro-postgres pg_dump -U postgres tc_agro_analytics > backup_$(shell date +%Y%m%d_%H%M%S).sql
	@echo "$(GREEN)✅ Backup created!$(NC)"

db-restore: ## Restore PostgreSQL database from backup (Usage: make db-restore FILE=backup.sql)
	@if [ -z "$(FILE)" ]; then \
		echo "$(RED)❌ Error: Please specify FILE=<backup.sql>$(NC)"; \
		exit 1; \
	fi
	@echo "$(YELLOW)🔄 Restoring database from $(FILE)...$(NC)"
	@docker exec -i tc-agro-postgres psql -U postgres tc_agro_analytics < $(FILE)
	@echo "$(GREEN)✅ Database restored!$(NC)"

rabbitmq-status: ## Show RabbitMQ status
	@echo "$(BLUE)🐰 RabbitMQ Status:$(NC)"
	@docker exec tc-agro-rabbitmq rabbitmqctl status

rabbitmq-queues: ## List RabbitMQ queues
	@echo "$(BLUE)📬 RabbitMQ Queues:$(NC)"
	@docker exec tc-agro-rabbitmq rabbitmqctl list_queues

redis-cli: ## Open Redis CLI
	@echo "$(BLUE)🔴 Connecting to Redis...$(NC)"
	@docker exec -it tc-agro-redis redis-cli

health: ## Check health of all services
	@echo "$(BLUE)🏥 Health Check:$(NC)"
	@docker ps --format "table {{.Names}}\t{{.Status}}"

stats: ## Show resource usage of containers
	@echo "$(BLUE)📈 Resource Usage:$(NC)"
	@docker stats --no-stream --format "table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}"
