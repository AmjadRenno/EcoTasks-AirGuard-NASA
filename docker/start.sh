#!/bin/bash

# EcoTasks AirGuard - Docker Start Script

echo "🚀 Starting EcoTasks AirGuard..."
echo ""

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Error: Docker is not running!"
    echo "Please start Docker Desktop and try again."
    exit 1
fi

echo "✅ Docker is running"
echo ""

# Navigate to docker directory
cd "$(dirname "$0")"

# Check if .env exists, if not copy from .env.example
if [ ! -f .env ]; then
    echo "📝 Creating .env file from .env.example..."
    cp .env.example .env
    echo "⚠️  Please update .env with your API keys if needed"
    echo ""
fi

echo "🔨 Building and starting containers..."
echo ""

# Build and start containers
docker compose up --build

echo ""
echo "✅ Containers stopped"
