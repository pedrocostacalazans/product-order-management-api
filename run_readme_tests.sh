#!/usr/bin/env bash

# Script para rodar todos os casos de teste descritos no README.md
# Uso: bash ./run_readme_tests.sh

BASE_URL=${BASE_URL:-"http://localhost:5086"}
API_PRODUCTS="$BASE_URL/api/products"
API_ORDERS="$BASE_URL/api/orders"

separator() {
  echo ""
  echo "============================================================"
  echo ""
}

wait_for_api() {
  echo "Verificando disponibilidade da API em $BASE_URL ..."
  local retries=30
  local count=0
  while (( count < retries )); do
    if curl -s -o /dev/null "$API_PRODUCTS"; then
      echo "API disponível."
      return 0
    fi
    count=$((count + 1))
    sleep 1
  done
  echo "Erro: não foi possível acessar a API em $BASE_URL após $retries tentativas." >&2
  exit 1
}

print_and_run() {
  local title="$1"; shift
  local cmd=("$@")
  separator
  echo "[TESTE] $title"
  echo "+ ${cmd[*]}"
  # Executa o comando exatamente como passado, permitindo variáveis já expandidas
  eval "${cmd[*]}"
  echo ""
}

# Executa e captura o ID do recurso do header Location (último segmento do path)
post_and_capture_id() {
  local title="$1"; shift
  local out_var_name="$1"; shift
  local cmd=("$@")
  separator
  echo "[TESTE] $title (capturando ID do Location)"
  echo "+ ${cmd[*]}"
  local resp
  # Usa -s para saída limpa, -i para incluir headers
  resp=$(eval "${cmd[*]}")
  # Imprime a resposta completa (headers + body)
  printf "%s\n" "$resp"
  # Extrai Location e o último segmento (GUID)
  local location
  location=$(printf "%s\n" "$resp" | awk '/^Location:/ {print $2}' | tr -d '\r')
  local id
  id=${location##*/}
  if [[ -n "$id" ]]; then
    printf -v "$out_var_name" "%s" "$id"
    echo "ID capturado para $title: $id"
  else
    echo "Aviso: não foi possível capturar o ID para $title (header Location ausente)." >&2
  fi
}

main() {
  echo "Iniciando testes do README contra: $BASE_URL"
  wait_for_api

  echo "\n=== Seção: Produtos ==="

  # 1. Criar produto válido (Laptop) e capturar PRODUCT_ID
  post_and_capture_id \
    "1. Criar produto válido (201 Created) - Laptop" \
    PRODUCT_ID \
    "curl -s -i -X POST $API_PRODUCTS -H 'Content-Type: application/json' -d '{\"name\":\"Laptop\",\"description\":\"13-inch\",\"price\":999.99,\"stockQuantity\":10}'"

  # 2. Criar outro produto válido (Mouse) e capturar MOUSE_ID
  post_and_capture_id \
    "2. Criar outro produto válido (201 Created) - Mouse" \
    MOUSE_ID \
    "curl -s -i -X POST $API_PRODUCTS -H 'Content-Type: application/json' -d '{\"name\":\"Mouse\",\"description\":\"Wireless optical mouse\",\"price\":29.99,\"stockQuantity\":50}'"

  # 3. Produto sem nome
  print_and_run \
    "3. Criar produto sem nome (400 Bad Request)" \
    "curl -s -i -X POST $API_PRODUCTS -H 'Content-Type: application/json' -d '{\"name\":\"\",\"description\":\"Invalid\",\"price\":10.0,\"stockQuantity\":5}'"

  # 4. Preço negativo
  print_and_run \
    "4. Criar produto com preço negativo (400 Bad Request)" \
    "curl -s -i -X POST $API_PRODUCTS -H 'Content-Type: application/json' -d '{\"name\":\"Free\",\"description\":\"Invalid negative price\",\"price\":-1.0,\"stockQuantity\":1}'"

  # 5. Estoque negativo
  print_and_run \
    "5. Criar produto com estoque negativo (400 Bad Request)" \
    "curl -s -i -X POST $API_PRODUCTS -H 'Content-Type: application/json' -d '{\"name\":\"BadStock\",\"description\":\"Negative stock\",\"price\":1.0,\"stockQuantity\":-5}'"

  # 6. Preço zero
  print_and_run \
    "6. Criar produto com preço zero (201 Created)" \
    "curl -s -i -X POST $API_PRODUCTS -H 'Content-Type: application/json' -d '{\"name\":\"Freebie\",\"description\":\"Zero price allowed\",\"price\":0.0,\"stockQuantity\":1}'"

  # 7. Listar produtos
  print_and_run \
    "7. Listar produtos (200 OK)" \
    "curl -s -i $API_PRODUCTS"

  # 8. Buscar produto inexistente (GUID válido)
  print_and_run \
    "8. Buscar produto inexistente (404 Not Found)" \
    "curl -s -i $API_PRODUCTS/00000000-0000-0000-0000-000000000000"

  echo "\n=== Seção: Pedidos ==="

  # 1. Criar pedido válido com 1 item (usar PRODUCT_ID) e capturar ORDER_ID
  if [[ -z "${PRODUCT_ID:-}" ]]; then
    echo "Aviso: PRODUCT_ID não definido; alguns testes de pedidos podem falhar." >&2
  fi
  post_and_capture_id \
    "1. Criar pedido válido com 1 item (201 Created)" \
    ORDER_ID \
    "curl -s -i -X POST $API_ORDERS -H 'Content-Type: application/json' -d '{\"customerName\":\"Alice\",\"items\":[{\"productId\":\"'"${PRODUCT_ID:-}"'\",\"quantity\":2}]}'"

  # 2. Criar pedido com múltiplos itens (PRODUCT_ID e MOUSE_ID)
  print_and_run \
    "2. Criar pedido com múltiplos itens (201 Created)" \
    "curl -s -i -X POST $API_ORDERS -H 'Content-Type: application/json' -d '{\"customerName\":\"Frank\",\"items\":[{\"productId\":\"'"${PRODUCT_ID:-}"'\",\"quantity\":1},{\"productId\":\"'"${MOUSE_ID:-}"'\",\"quantity\":3}]}'"

  # 3. Nome do cliente vazio
  print_and_run \
    "3. Nome do cliente vazio (400 Bad Request)" \
    "curl -s -i -X POST $API_ORDERS -H 'Content-Type: application/json' -d '{\"customerName\":\"\",\"items\":[{\"productId\":\"'"${PRODUCT_ID:-}"'\",\"quantity\":1}]}'"

  # 4. Pedido sem itens
  print_and_run \
    "4. Pedido sem itens (400 Bad Request)" \
    "curl -s -i -X POST $API_ORDERS -H 'Content-Type: application/json' -d '{\"customerName\":\"Bob\",\"items\":[]}'"

  # 5. Produto inexistente (GUID zero)
  print_and_run \
    "5. Produto inexistente (400 Bad Request)" \
    "curl -s -i -X POST $API_ORDERS -H 'Content-Type: application/json' -d '{\"customerName\":\"Carol\",\"items\":[{\"productId\":\"00000000-0000-0000-0000-000000000000\",\"quantity\":1}]}'"

  # 6. Quantidade zero
  print_and_run \
    "6. Quantidade zero (400 Bad Request)" \
    "curl -s -i -X POST $API_ORDERS -H 'Content-Type: application/json' -d '{\"customerName\":\"Eve\",\"items\":[{\"productId\":\"'"${PRODUCT_ID:-}"'\",\"quantity\":0}]}'"

  # 7. Quantidade acima do estoque
  print_and_run \
    "7. Quantidade acima do estoque (400 Bad Request)" \
    "curl -s -i -X POST $API_ORDERS -H 'Content-Type: application/json' -d '{\"customerName\":\"Dave\",\"items\":[{\"productId\":\"'"${PRODUCT_ID:-}"'\",\"quantity\":1000}]}'"

  # 8. Buscar pedido existente por ID (usar ORDER_ID)
  if [[ -n "${ORDER_ID:-}" ]]; then
    print_and_run \
      "8. Buscar pedido existente por ID (200 OK)" \
      "curl -s -i $API_ORDERS/$ORDER_ID"
  else
    echo "Aviso: ORDER_ID não definido; pulando teste 8 (buscar pedido existente)." >&2
  fi

  # 9. Buscar pedido com GUID inválido
  print_and_run \
    "9. Buscar pedido com GUID inválido (404 Not Found)" \
    "curl -s -i $API_ORDERS/not-a-guid"

  # 10. Buscar pedido inexistente (GUID zero)
  print_and_run \
    "10. Buscar pedido inexistente (404 Not Found)" \
    "curl -s -i $API_ORDERS/00000000-0000-0000-0000-000000000000"

  # 11. Novo pedido acima do estoque restante
  print_and_run \
    "11. Novo pedido acima do estoque restante (400 Bad Request esperado)" \
    "curl -s -i -X POST $API_ORDERS -H 'Content-Type: application/json' -d '{\"customerName\":\"Gina\",\"items\":[{\"productId\":\"'"${PRODUCT_ID:-}"'\",\"quantity\":9}]}'"

  separator
  echo "Concluído. Todos os comandos foram executados."
}

main "$@"