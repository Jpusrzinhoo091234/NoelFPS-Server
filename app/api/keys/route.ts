// app/api/keys/route.ts
import { NextResponse } from 'next/server';

export async function GET() {
  // Em um ambiente de produção, estas chaves seriam carregadas de variáveis de ambiente
  // Por exemplo: process.env.MY_SECRET_KEY_1
  const keys = {
    key1: process.env.MY_SECRET_KEY_1 || 'default_key_1_if_not_set',
    key2: process.env.MY_SECRET_KEY_2 || 'default_key_2_if_not_set',
    // Adicione mais chaves conforme necessário
  };

  return NextResponse.json(keys);
}
