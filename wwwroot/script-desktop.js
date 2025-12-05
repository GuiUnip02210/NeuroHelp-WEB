/* ===========================================================
   🔧 FUNÇÕES UTILITÁRIAS (HELPERS)
   =========================================================== */
const $ = (sel, p = document) => p.querySelector(sel);
const $$ = (sel, p = document) => [...p.querySelectorAll(sel)];
const save = (key, value) => localStorage.setItem(key, JSON.stringify(value));
const load = (key, fallback = []) => {
  try {
    const data = localStorage.getItem(key);
    return data ? JSON.parse(data) : fallback;
  } catch {
    return fallback;
  }
};
const toast = (msg) => alert(msg);

/**
 * Atrasa a execução de uma função (para barras de busca)
 * @param {Function} func - A função a ser executada.
 * @param {number} delay - O tempo em milissegundos.
 */
function debounce(func, delay = 300) {
  let timeout;
  return (...args) => {
    clearTimeout(timeout);
    timeout = setTimeout(() => {
      func.apply(this, args);
    }, delay);
  };
}

// ===========================================================
// INÍCIO - LÓGICA DE TEMA (DARK MODE)
// ===========================================================

/**
 * Aplica o tema (light/dark) salvo assim que o script carrega.
 * Previne o "flash" de tema incorreto.
 */
function applyInitialTheme() {
  // Usamos a função 'load' existente, default 'light'
  const savedTheme = load('theme-preference', 'light'); 
  if (savedTheme === 'dark') {
    document.body.classList.add('dark-mode');
  } else {
    document.body.classList.remove('dark-mode');
  }
}
// Chama imediatamente para aplicar o tema ANTES da página renderizar
applyInitialTheme();

// ===========================================================
// FIM - LÓGICA DE TEMA (DARK MODE)
// ===========================================================

/* URL base da API */
const API_BASE = ""; // URLs relativas - mesmo servidor

/* ===========================================================
   🔐 LOGIN
   =========================================================== */
function initLogin() {
  const form = $("#login-form");
  if (!form) return;

  form.addEventListener("submit", async (e) => {
    e.preventDefault();

    const email = $("#email").value.trim().toLowerCase();
    const password = $("#password").value.trim();
    console.log("Senha lida do campo:", password);
    if (!email || !password) return toast("Preencha todos os campos.");

    try {
      const response = await fetch(`${API_BASE}/api/usuarios/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          Email: email,
          Senha: password
        })
      });

      if (response.ok) {
        const data = await response.json();
        
        // Guardar o token no sessionStorage
        if (data.token) {
          sessionStorage.setItem('authToken', data.token);
        }
        
        toast("Login realizado com sucesso!");
        
        // Redirecionar para o histórico (única página disponível)
        window.location.href = "user-dashboard-historico.html";
      } else {
        // Tratar erro de autenticação
        let errorMessage = "E-mail ou senha incorretos.";
        try {
          const errorData = await response.json();
          if (errorData.message) {
            errorMessage = errorData.message;
          }
        } catch (e) {
          // Se não conseguir ler a mensagem de erro, usar a mensagem padrão
        }
        toast(errorMessage);
      }
    } catch (error) {
      // Tratar erros de rede ou outros problemas
      console.error('Erro na autenticação:', error);
      toast("Erro de conexão. Verifique sua internet e tente novamente.");
    }
  });
}

/* ===========================================================
   🧾 CADASTRO (Atualizado para API)
   =========================================================== */
async function initRegister() {
  const form = $("#register-form");
  if (!form) return;
  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    const nomeCompleto = $("#r-name").value.trim();
    const email = $("#r-email").value.trim().toLowerCase();
    const senha = $("#r-pass").value.trim();
    const confirmarSenha = $("#r-confirm").value.trim();
    if (!nomeCompleto || !email || !senha || !confirmarSenha) {
      return toast("Preencha todos os campos.");
    }
    if (senha !== confirmarSenha) {
      return toast("As senhas não coincidem.");
    }
    const submitButton = form.querySelector("button[type='submit']");
    submitButton.disabled = true;
    submitButton.textContent = "A registar...";
    try {
      const response = await fetch(`${API_BASE}/api/usuarios/registrar`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          NomeCompleto: nomeCompleto,
          Email: email,
          Senha: senha
        })
      });
      if (response.ok) {
        toast("Conta criada com sucesso! Por favor, faça o login.");
        go("/");
      } else {
        const errorData = await response.json();
        // Tenta extrair a mensagem de erro específica (ex: "Email já está em uso")
        const errorMessage = errorData?.message || errorData?.title || "Erro ao criar conta.";
        toast(errorMessage);
      }
    } catch (error) {
      console.error('Erro no registo:', error);
      toast("Erro de conexão ao tentar registar.");
    } finally {
      submitButton.disabled = false;
      submitButton.textContent = "Criar conta";
    }
  });
}

/* ===========================================================
   🔧 CONFIGURAÇÕES
   =========================================================== */
function initConfig() {
  const logoutBtn = $("#nav-logout");
  if (logoutBtn) {
    logoutBtn.addEventListener("click", (e) => {
      e.preventDefault();
      sessionStorage.removeItem('authToken');
      sessionStorage.removeItem('currentTicketId');
      localStorage.removeItem('user');
      window.location.href = '/';
    });
  }
}

/* ===========================================================
   👤 SAUDAÇÃO DO USUÁRIO
   =========================================================== */
function atualizarSaudacaoUsuario() {
  const token = sessionStorage.getItem('authToken');
  if (!token) return;

  try {
    const payload = decodeJWT(token);
    const nameIdentifierClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
    const nameClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";

    if (payload && payload[nameClaim]) {
      const userName = payload[nameClaim];
      const userInfoElement = document.querySelector('.user-info');
      if (userInfoElement) {
        userInfoElement.textContent = `Olá, ${userName}`;
      }
    }
  } catch (error) {
    console.error('Erro ao decodificar token para saudação:', error);
  }
}

/**
 * Decodifica um token JWT (apenas o payload)
 */
function decodeJWT(token) {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) {
      throw new Error('Token JWT inválido');
    }
    
    const payload = parts[1];
    const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
    return JSON.parse(decoded);
  } catch (error) {
    console.error('Erro ao decodificar JWT:', error);
    return null;
  }
}

/* ===========================================================
   🎨 TEMA SWITCHER
   =========================================================== */
function initThemeSwitcher() {
  const themeSwitcher = document.getElementById('theme-switcher');
  if (!themeSwitcher) return;

  // Aplicar tema salvo
  const savedTheme = load('theme-preference', 'light');
  if (savedTheme === 'dark') {
    document.body.classList.add('dark-mode');
    themeSwitcher.checked = true;
  }

  // Event listener para mudança de tema
  themeSwitcher.addEventListener('change', function() {
    if (this.checked) {
      document.body.classList.add('dark-mode');
      save('theme-preference', 'dark');
    } else {
      document.body.classList.remove('dark-mode');
      save('theme-preference', 'light');
    }
  });
}

/* ===========================================================
   👁️ TOGGLE DE SENHA
   =========================================================== */
function initPasswordToggles() {
  const toggleButtons = document.querySelectorAll('.toggle-btn');
  
  toggleButtons.forEach(button => {
    button.addEventListener('click', function() {
      const targetId = this.getAttribute('data-target');
      const targetInput = document.getElementById(targetId);
      
      if (targetInput) {
        if (targetInput.type === 'password') {
          targetInput.type = 'text';
          this.textContent = '🙈';
        } else {
          targetInput.type = 'password';
          this.textContent = '👁️';
        }
      }
    });
  });
}

/* ===========================================================
   🚀 NAVEGAÇÃO
   =========================================================== */
function go(path) {
  window.location.href = path;
}

/* ===========================================================
   📋 SISTEMA DE HISTÓRICO DE CHAMADOS (SOMENTE LEITURA)
   =========================================================== */

// Variáveis globais para paginação e filtros
let currentPage = 1;
let totalPages = 1;
let pageSize = 10;
let currentFilters = {};

/**
 * Inicializa o visualizador de histórico
 */
function initHistoricoViewer() {
  console.log('Inicializando visualizador de histórico...');
  
  // Verificar autenticação
  const token = sessionStorage.getItem('authToken');
  if (!token) {
    console.log('Token não encontrado, redirecionando para login');
    go('/');
    return;
  }

  // Configurar event listeners
  setupEventListeners();
  
  // Carregar dados iniciais
  loadHistoricoData();
  loadHistoricoStats();
}

/**
 * Configura os event listeners
 */
function setupEventListeners() {
  // Botões de paginação
  const btnPrevPage = $('#btn-prev-page');
  const btnNextPage = $('#btn-next-page');
  
  if (btnPrevPage) {
    btnPrevPage.addEventListener('click', () => {
      if (currentPage > 1) {
        currentPage--;
        loadHistoricoData();
      }
    });
  }
  
  if (btnNextPage) {
    btnNextPage.addEventListener('click', () => {
      if (currentPage < totalPages) {
        currentPage++;
        loadHistoricoData();
      }
    });
  }

  // Botões de filtro
  const btnFiltrar = $('#btn-filtrar');
  const btnLimpar = $('#btn-limpar');
  
  if (btnFiltrar) {
    btnFiltrar.addEventListener('click', aplicarFiltros);
  }
  
  if (btnLimpar) {
    btnLimpar.addEventListener('click', limparFiltros);
  }

  // Modal
  const modal = $('#modal-detalhes');
  const modalClose = $('#modal-close');
  
  if (modalClose && modal) {
    modalClose.addEventListener('click', () => {
      modal.style.display = 'none';
    });

    window.addEventListener('click', (e) => {
      if (e.target === modal) {
        modal.style.display = 'none';
      }
    });
  }

  // Filtros com debounce para campos de texto
  const filterInputs = ['#filter-id'];
  filterInputs.forEach(selector => {
    const input = $(selector);
    if (input) {
      input.addEventListener('input', debounce(() => {
        aplicarFiltros();
      }, 500));
    }
  });
}

/**
 * Carrega os dados do histórico
 */
async function loadHistoricoData() {
  const token = sessionStorage.getItem('authToken');
  if (!token) {
    go('/');
    return;
  }

  const tbody = $('#historico-tbody');
  if (!tbody) return;

  try {
    // Mostrar loading
    tbody.innerHTML = '<tr><td colspan="8" class="loading">Carregando dados...</td></tr>';

    // Construir URL com filtros e paginação
    const params = new URLSearchParams({
      page: currentPage,
      pageSize: pageSize,
      ...currentFilters
    });

    const response = await fetch(`${API_BASE}/api/historico-chamados?${params}`, {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });

    if (!response.ok) {
      let errorDetails = `${response.status}: ${response.statusText}`;
      try {
        const errorData = await response.text();
        if (errorData) {
          errorDetails += ` - ${errorData}`;
        }
      } catch (e) {
        // Se não conseguir ler o corpo da resposta, usar apenas status
      }
      throw new Error(errorDetails);
    }

    const data = await response.json();

    // Atualizar informações de paginação
    totalPages = data.totalPages || 1;
    updatePaginationInfo(data);

    // Renderizar dados
    renderHistoricoTable(data.data || []);

  } catch (error) {
    console.error('Erro ao carregar dados do histórico:', error);
    console.error('URL da requisição:', `${API_BASE}/api/historico-chamados?${params}`);
    console.error('Filtros aplicados:', currentFilters);
    
    let errorMessage = 'Erro ao carregar dados. Tente novamente.';
    if (error.message) {
      errorMessage = `Erro: ${error.message}`;
    }
    
    tbody.innerHTML = `<tr><td colspan="8" class="error">${errorMessage}</td></tr>`;
  }
}

/**
 * Renderiza a tabela de histórico
 */
function renderHistoricoTable(chamados) {
  const tbody = $('#historico-tbody');
  if (!tbody) return;

  if (!chamados || chamados.length === 0) {
    tbody.innerHTML = '<tr><td colspan="8" class="no-data">Nenhum chamado encontrado.</td></tr>';
    return;
  }

  tbody.innerHTML = chamados.map(chamado => `
    <tr>
      <td>${chamado.idDoCaso || '-'}</td>
      <td>${truncateText(chamado.tipo, 20)}</td>
      <td>${truncateText(chamado.resumo, 30)}</td>
      <td>${truncateText(chamado.categoria, 25)}</td>
      <td><span class="status ${getStatusClass(chamado.status)}">${chamado.status || '-'}</span></td>
      <td><span class="priority ${getPriorityClass(chamado.prioridade)}">${chamado.prioridade || '-'}</span></td>
      <td>${formatDate(chamado.dataAbertura)}</td>
      <td>
        <button class="btn btn-small" onclick="verDetalhes(${chamado.idDoCaso})">
          👁️ Ver
        </button>
      </td>
    </tr>
  `).join('');
}

/**
 * Atualiza as informações de paginação
 */
function updatePaginationInfo(data) {
  const pageInfo = $('#page-info');
  const statTotal = $('#stat-total');
  const btnPrevPage = $('#btn-prev-page');
  const btnNextPage = $('#btn-next-page');

  if (pageInfo) {
    pageInfo.textContent = `Página ${currentPage} de ${totalPages}`;
  }

  if (statTotal) {
    statTotal.textContent = data.totalRecords || 0;
  }

  // Habilitar/desabilitar botões de paginação
  if (btnPrevPage) {
    btnPrevPage.disabled = currentPage <= 1;
  }

  if (btnNextPage) {
    btnNextPage.disabled = currentPage >= totalPages;
  }
}

/**
 * Aplica os filtros selecionados
 */
function aplicarFiltros() {
  currentFilters = {};
  currentPage = 1;

  // Coletar valores dos filtros
  const filterId = $('#filter-id');
  const filterCategoria = $('#filter-categoria');
  const filterStatus = $('#filter-status');
  const filterPrioridade = $('#filter-prioridade');
  const filterDataInicio = $('#filter-data-inicio');
  const filterDataFim = $('#filter-data-fim');

  if (filterId && filterId.value) {
    currentFilters.idDoCaso = filterId.value;
  }

  if (filterCategoria && filterCategoria.value) {
    currentFilters.categoria = filterCategoria.value;
  }

  if (filterStatus && filterStatus.value) {
    currentFilters.status = filterStatus.value;
  }

  if (filterPrioridade && filterPrioridade.value) {
    currentFilters.prioridade = filterPrioridade.value;
  }

  if (filterDataInicio && filterDataInicio.value) {
    // Validar formato da data
    const dataInicio = new Date(filterDataInicio.value);
    if (!isNaN(dataInicio.getTime())) {
      currentFilters.dataAberturaInicio = filterDataInicio.value;
    } else {
      toast("Data de início inválida");
      return;
    }
  }

  if (filterDataFim && filterDataFim.value) {
    // Validar formato da data
    const dataFim = new Date(filterDataFim.value);
    if (!isNaN(dataFim.getTime())) {
      currentFilters.dataAberturaFim = filterDataFim.value;
    } else {
      toast("Data de fim inválida");
      return;
    }
  }

  // Validar se data início não é maior que data fim
  if (currentFilters.dataAberturaInicio && currentFilters.dataAberturaFim) {
    const inicio = new Date(currentFilters.dataAberturaInicio);
    const fim = new Date(currentFilters.dataAberturaFim);
    if (inicio > fim) {
      toast("Data de início não pode ser maior que data de fim");
      return;
    }
  }

  loadHistoricoData();
}

/**
 * Limpa todos os filtros
 */
function limparFiltros() {
  currentFilters = {};
  currentPage = 1;

  // Limpar campos de filtro
  const filterInputs = [
    '#filter-id', '#filter-categoria', '#filter-status', '#filter-prioridade',
    '#filter-data-inicio', '#filter-data-fim'
  ];

  filterInputs.forEach(selector => {
    const input = $(selector);
    if (input) {
      input.value = '';
    }
  });

  loadHistoricoData();
}

/**
 * Carrega estatísticas para popular os filtros
 */
async function loadHistoricoStats() {
  const token = sessionStorage.getItem('authToken');
  if (!token) return;

  try {
    const response = await fetch(`${API_BASE}/api/historico-chamados/stats`, {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });

    if (!response.ok) {
      console.error('Erro ao carregar estatísticas:', response.status);
      return;
    }

    const stats = await response.json();

    // Atualizar total de chamados
    const statTotal = $('#stat-total');
    if (statTotal) {
      statTotal.textContent = stats.totalChamados || 0;
    }

    // Preencher filtros com dados únicos
    populateFilterOptions(stats);

  } catch (error) {
    console.error('Erro ao carregar estatísticas:', error);
  }
}

/**
 * Popula as opções dos filtros com dados únicos
 */
function populateFilterOptions(stats) {
  // Categorias
  const filterCategoria = $('#filter-categoria');
  if (filterCategoria && stats.chamadosPorCategoria) {
    // Limpar opções existentes (exceto a primeira)
    while (filterCategoria.children.length > 1) {
      filterCategoria.removeChild(filterCategoria.lastChild);
    }
    
    const categorias = stats.chamadosPorCategoria.map(item => item.categoria).sort();
    categorias.forEach(categoria => {
      const option = document.createElement('option');
      option.value = categoria;
      option.textContent = categoria;
      filterCategoria.appendChild(option);
    });
  }
  
  // Status
  const filterStatus = $('#filter-status');
  if (filterStatus && stats.chamadosPorStatus) {
    // Limpar opções existentes (exceto a primeira)
    while (filterStatus.children.length > 1) {
      filterStatus.removeChild(filterStatus.lastChild);
    }
    
    const statusList = stats.chamadosPorStatus.map(item => item.status).sort();
    statusList.forEach(status => {
      const option = document.createElement('option');
      option.value = status;
      option.textContent = status;
      filterStatus.appendChild(option);
    });
  }
  
  // Prioridades
  const filterPrioridade = $('#filter-prioridade');
  if (filterPrioridade && stats.chamadosPorPrioridade) {
    // Limpar opções existentes (exceto a primeira)
    while (filterPrioridade.children.length > 1) {
      filterPrioridade.removeChild(filterPrioridade.lastChild);
    }
    
    const prioridades = stats.chamadosPorPrioridade.map(item => item.prioridade).sort();
    prioridades.forEach(prioridade => {
      const option = document.createElement('option');
      option.value = prioridade;
      option.textContent = prioridade;
      filterPrioridade.appendChild(option);
    });
  }
}

/**
 * Funções utilitárias para o histórico
 */
function truncateText(text, maxLength) {
  if (!text) return '-';
  return text.length > maxLength ? text.substring(0, maxLength) + '...' : text;
}

function formatDate(dateString) {
  if (!dateString) return '-';
  try {
    const date = new Date(dateString);
    return date.toLocaleDateString('pt-BR') + ' ' + date.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
  } catch {
    return dateString;
  }
}

function getStatusClass(status) {
  if (!status) return 'default';
  const statusLower = status.toLowerCase();
  if (statusLower.includes('aberto') || statusLower.includes('novo')) return 'open';
  if (statusLower.includes('andamento') || statusLower.includes('progresso')) return 'progress';
  if (statusLower.includes('fechado') || statusLower.includes('resolvido')) return 'closed';
  return 'default';
}

function getPriorityClass(priority) {
  if (!priority) return 'default';
  const priorityLower = priority.toLowerCase();
  if (priorityLower.includes('alta') || priorityLower.includes('urgente')) return 'high';
  if (priorityLower.includes('média') || priorityLower.includes('normal')) return 'medium';
  if (priorityLower.includes('baixa')) return 'low';
  return 'default';
}

/**
 * Mostra detalhes de um chamado
 */
async function verDetalhes(idChamado) {
  const token = sessionStorage.getItem('authToken');
  if (!token) {
    go('/');
    return;
  }

  const modal = $('#modal-detalhes');
  const modalBody = $('#modal-body');
  
  if (!modal || !modalBody) return;

  try {
    modalBody.innerHTML = '<p>Carregando detalhes...</p>';
    modal.style.display = 'block';

    const response = await fetch(`${API_BASE}/api/historico-chamados/${idChamado}`, {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });

    if (!response.ok) {
      throw new Error(`Erro ${response.status}: ${response.statusText}`);
    }

    const chamado = await response.json();

    modalBody.innerHTML = `
      <div class="detail-grid">
        <div class="detail-item">
          <strong>ID do Caso:</strong>
          <span>${chamado.idDoCaso || '-'}</span>
        </div>
        <div class="detail-item">
          <strong>Tipo:</strong>
          <span>${chamado.tipo || '-'}</span>
        </div>
        <div class="detail-item">
          <strong>Resumo:</strong>
          <span>${chamado.resumo || '-'}</span>
        </div>
        <div class="detail-item">
          <strong>Categoria:</strong>
          <span>${chamado.categoria || '-'}</span>
        </div>
        <div class="detail-item">
          <strong>Status:</strong>
          <span class="status ${getStatusClass(chamado.status)}">${chamado.status || '-'}</span>
        </div>
        <div class="detail-item">
          <strong>Prioridade:</strong>
          <span class="priority ${getPriorityClass(chamado.prioridade)}">${chamado.prioridade || '-'}</span>
        </div>
        <div class="detail-item">
          <strong>Data Abertura:</strong>
          <span>${formatDate(chamado.dataAbertura)}</span>
        </div>
        <div class="detail-item">
          <strong>Data Resolução:</strong>
          <span>${formatDate(chamado.dataResolucao)}</span>
        </div>
        <div class="detail-item">
          <strong>Relatado Por:</strong>
          <span>${chamado.relatadoPor || '-'}</span>
        </div>
        <div class="detail-item">
          <strong>Atribuído:</strong>
          <span>${chamado.atribuido || '-'}</span>
        </div>
        <div class="detail-item full-width">
          <strong>Descrição:</strong>
          <div class="description-box">${chamado.descricao || '-'}</div>
        </div>
        <div class="detail-item full-width">
          <strong>Descrição da Solução:</strong>
          <div class="description-box">${chamado.descricaoSolucao || '-'}</div>
        </div>
      </div>
    `;

  } catch (error) {
    console.error('Erro ao carregar detalhes:', error);
    modalBody.innerHTML = '<p class="error">Erro ao carregar detalhes do chamado.</p>';
  }
}

/* ===========================================================
   🚀 INICIALIZAÇÃO GLOBAL
   =========================================================== */
document.addEventListener("DOMContentLoaded", () => {
  const path = window.location.pathname;

  if (path.endsWith("index.html") || path === "/" || path === "") {
    initLogin();
    initPasswordToggles();
  } else if (path.endsWith("cadastro-desktop.html")) {
    initRegister();
    initPasswordToggles();
  } else if (path.endsWith("config-desktop.html")) {
    initConfig();
    atualizarSaudacaoUsuario();
    initThemeSwitcher();
  } else if (path.endsWith("user-dashboard-historico.html")) {
    initHistoricoViewer();
    initConfig();
    atualizarSaudacaoUsuario();
  }
});