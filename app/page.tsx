// app/page.tsx
"use client"; // Marca este componente como um Client Component

import { useEffect, useState } from 'react';

interface Keys {
  key1: string;
  key2: string;
  // Adicione mais chaves conforme necessário
}

export default function Home() {
  const [keys, setKeys] = useState<Keys | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchKeys() {
      try {
        const response = await fetch('/api/keys');
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data: Keys = await response.json();
        setKeys(data);
      } catch (e: any) {
        setError(e.message);
      } finally {
        setLoading(false);
      }
    }

    fetchKeys();
  }, []);

  if (loading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-zinc-50 dark:bg-black">
        <p className="text-lg text-black dark:text-white">Carregando chaves...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-zinc-50 dark:bg-black">
        <p className="text-lg text-red-500">Erro ao carregar chaves: {error}</p>
      </div>
    );
  }

  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-zinc-50 p-8 font-sans dark:bg-black">
      <h1 className="mb-8 text-4xl font-bold text-black dark:text-white">Minhas Chaves Secretas</h1>
      <div className="rounded-lg bg-white p-6 shadow-md dark:bg-gray-800">
        {keys ? (
          <ul className="list-disc pl-5 text-black dark:text-white">
            <li><strong>Key 1:</strong> {keys.key1}</li>
            <li><strong>Key 2:</strong> {keys.key2}</li>
            {/* Renderize mais chaves aqui */}
          </ul>
        ) : (
          <p className="text-black dark:text-white">Nenhuma chave encontrada.</p>
        )}
      </div>
      <p className="mt-4 text-sm text-gray-600 dark:text-gray-400">
        Estas chaves são carregadas via API e não estão expostas no código fonte do frontend.
      </p>
    </div>
  );
}
