// app/api/keys/route.ts
import { NextResponse } from 'next/server';
import { NextRequest } from 'next/server'; // Importar NextRequest para acessar os headers

export async function GET(request: NextRequest) { // Receber o objeto request
  // A chave de acesso à API deve ser definida como uma variável de ambiente no Vercel
  const API_ACCESS_KEY = process.env.API_ACCESS_KEY;

  // Verificar se a chave de acesso à API está configurada
  if (!API_ACCESS_KEY) {
    console.error('API_ACCESS_KEY não está configurada nas variáveis de ambiente.');
    return new NextResponse(JSON.stringify({ error: 'Configuração de API Key ausente.' }), {
      status: 500,
      headers: {
        'Content-Type': 'application/json',
      },
    });
  }

  // Obter o cabeçalho de autorização da requisição
  const authorizationHeader = request.headers.get('authorization');

  // Verificar se o cabeçalho de autorização está presente e no formato correto
  if (!authorizationHeader || !authorizationHeader.startsWith('Bearer ')) {
    return new NextResponse(JSON.stringify({ error: 'Acesso não autorizado. Cabeçalho Authorization Bearer ausente ou inválido.' }), {
      status: 401,
      headers: {
        'Content-Type': 'application/json',
      },
    });
  }

  // Extrair o token (chave de API) do cabeçalho
  const token = authorizationHeader.split(' ')[1];

  // Comparar o token fornecido com a chave de acesso esperada
  if (token !== API_ACCESS_KEY) {
    return new NextResponse(JSON.stringify({ error: 'Acesso não autorizado. Chave de API inválida.' }), {
      status: 401,
      headers: {
        'Content-Type': 'application/json',
      },
    });
  }

  // Se a autenticação for bem-sucedida, retorne as chaves secretas
  const keys = {
    key1: process.env.MY_SECRET_KEY_1 || 'default_key_1_if_not_set',
    key2: process.env.MY_SECRET_KEY_2 || 'default_key_2_if_not_set',
    // Adicione mais chaves conforme necessário
  };

  return NextResponse.json(keys);
}
