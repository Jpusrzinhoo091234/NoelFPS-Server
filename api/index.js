const express = require('express');
const cors = require('cors');
const bodyParser = require('body-parser');
const fs = require('fs');
const path = require('path');
require('dotenv').config();

const app = express();
const PORT = process.env.PORT || 3000;
const API_KEY = process.env.API_KEY;
if (!API_KEY) {
    throw new Error('Variável de ambiente API_KEY não configurada.');
}

const DATA_DIR = process.env.DATA_DIR || path.join(__dirname, 'data');
const DATA_FILE = process.env.DATA_FILE || path.join(DATA_DIR, 'licenses.json');

app.use(cors());
app.use(bodyParser.json());

// Middleware de segurança simples
const authenticate = (req, res, next) => {
    const key = req.headers['x-api-key'];
    if (key && key === API_KEY) {
        next();
    } else {
        res.status(401).json({ error: 'Não autorizado' });
    }
};

// Funções utilitárias para ler/escrever dados
const readData = () => {
    if (!fs.existsSync(DATA_FILE)) {
        return [];
    }
    const data = fs.readFileSync(DATA_FILE, 'utf8');
    return JSON.parse(data);
};

const writeData = (data) => {
    const dir = path.dirname(DATA_FILE);
    if (!fs.existsSync(dir)) {
        fs.mkdirSync(dir, { recursive: true });
    }
    fs.writeFileSync(DATA_FILE, JSON.stringify(data, null, 2), 'utf8');
};

// Rota para validar/ativar uma chave
app.post('/api/validate', authenticate, (req, res) => {
    const { key, hwid, minutos } = req.body;
    const licenses = readData();
    const licenseIndex = licenses.findIndex(l => l.key === key);

    if (licenseIndex === -1) {
        return res.status(404).json({ success: false, message: 'Chave não encontrada.' });
    }

    let license = licenses[licenseIndex];
    const now = new Date();

    // Se a chave já foi usada
    if (license.usada) {
        if (license.hwid !== hwid) {
            return res.status(403).json({ success: false, message: 'Chave vinculada a outro computador.' });
        }

        if (license.expiraEm && new Date(license.expiraEm) < now) {
            return res.status(403).json({ success: false, message: 'Chave expirada.' });
        }

        return res.json({ success: true, message: 'Bem-vindo de volta!', license });
    }

    // Se não foi usada, ativa agora
    license.usada = true;
    license.hwid = hwid;
    const expirationDate = new Date(now.getTime() + (license.minutos || minutos || 0) * 60000);
    license.expiraEm = expirationDate.toISOString();

    licenses[licenseIndex] = license;
    writeData(licenses);

    res.json({ success: true, message: 'Chave ativada com sucesso!', license });
});

// Rota para pegar todas as chaves (apenas para o futuro painel ADM)
app.get('/api/licenses', authenticate, (req, res) => {
    res.json(readData());
});

// Rota para criar uma nova chave (ADM)
app.post('/api/licenses', authenticate, (req, res) => {
    const newLicense = req.body;
    const licenses = readData();
    licenses.push(newLicense);
    writeData(licenses);
    res.status(201).json(newLicense);
});

app.listen(PORT, () => {
    console.log(`Servidor rodando na porta ${PORT}`);
});
