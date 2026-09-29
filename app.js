// Smart Finance Tracer Interactive Application Engine
document.addEventListener('DOMContentLoaded', () => {
  // State
  let currentMode = 'Personal'; // 'Personal' or 'Office'
  let currentMonth = '2026-09';

  // Seed Data for Simulator
  const personalTransactions = [
    { id: 1, title: 'Monthly Salary Deposit', type: 'Income', category: 'Salary', amount: 5500, date: '2026-09-01' },
    { id: 2, title: 'Freelance Mobile App UI', type: 'Income', category: 'Freelance', amount: 1700, date: '2026-09-08' },
    { id: 3, title: 'Apartment Monthly Rent', type: 'Expense', category: 'Housing', amount: 1650, date: '2026-09-03' },
    { id: 4, title: 'Supermarket Groceries', type: 'Expense', category: 'Groceries', amount: 480, date: '2026-09-12' },
    { id: 5, title: 'High-Speed Fiber Internet', type: 'Expense', category: 'Utilities', amount: 90, date: '2026-09-15' },
    { id: 6, title: 'Weekend Dining & Coffee', type: 'Expense', category: 'Food & Dining', amount: 320, date: '2026-09-20' },
    { id: 7, title: 'Health Insurance Premium', type: 'Expense', category: 'Healthcare', amount: 380, date: '2026-09-22' },
    { id: 8, title: 'Fitness Gym Membership', type: 'Expense', category: 'Fitness', amount: 60, date: '2026-09-25' }
  ];

  const officeTransactions = [
    { id: 101, title: 'Enterprise Retainer - Nexus Corp', type: 'Income', category: 'B2B Sales', amount: 32500, date: '2026-09-02' },
    { id: 102, title: 'Software License Renewal - FinTech', type: 'Income', category: 'Subscriptions', amount: 14200, date: '2026-09-10' },
    { id: 103, title: 'Engineering & Product Payroll', type: 'Expense', category: 'Payroll', amount: 18500, date: '2026-09-05' },
    { id: 104, title: 'Commercial Office Suite Lease', type: 'Expense', category: 'Office Lease', amount: 6200, date: '2026-09-04' },
    { id: 105, title: 'AWS Cloud Infrastructure Cluster', type: 'Expense', category: 'Cloud & Hosting', amount: 2450, date: '2026-09-14' },
    { id: 106, title: 'Corporate Legal & Regulatory Audit', type: 'Expense', category: 'Legal & Audit', amount: 1800, date: '2026-09-18' },
    { id: 107, title: 'B2B Product Launch Marketing Campaign', type: 'Expense', category: 'Marketing', amount: 3100, date: '2026-09-24' }
  ];

  const categories = {
    Personal: [
      { name: 'Salary', icon: '💰', color: '#10b981', defaultType: 'Income' },
      { name: 'Freelance', icon: '💻', color: '#3b82f6', defaultType: 'Income' },
      { name: 'Housing', icon: '🏠', color: '#ef4444', defaultType: 'Expense' },
      { name: 'Groceries', icon: '🛒', color: '#f59e0b', defaultType: 'Expense' },
      { name: 'Food & Dining', icon: '🍽️', color: '#ec4899', defaultType: 'Expense' },
      { name: 'Utilities', icon: '⚡', color: '#8b5cf6', defaultType: 'Expense' },
      { name: 'Healthcare', icon: '🏥', color: '#06b6d4', defaultType: 'Expense' },
      { name: 'Fitness', icon: '🏋️', color: '#14b8a6', defaultType: 'Expense' }
    ],
    Office: [
      { name: 'B2B Sales', icon: '🏢', color: '#10b981', defaultType: 'Income' },
      { name: 'Subscriptions', icon: '📜', color: '#3b82f6', defaultType: 'Income' },
      { name: 'Payroll', icon: '👥', color: '#ef4444', defaultType: 'Expense' },
      { name: 'Office Lease', icon: '🏬', color: '#f59e0b', defaultType: 'Expense' },
      { name: 'Cloud & Hosting', icon: '☁️', color: '#8b5cf6', defaultType: 'Expense' },
      { name: 'Marketing', icon: '📣', color: '#ec4899', defaultType: 'Expense' },
      { name: 'Legal & Audit', icon: '⚖️', color: '#6366f1', defaultType: 'Expense' }
    ]
  };

  // Smart Categorization Keywords
  const smartRules = [
    { keywords: ['salary', 'paycheck', 'wage'], category: 'Salary', type: 'Income' },
    { keywords: ['client', 'b2b', 'contract', 'deal', 'nexus'], category: 'B2B Sales', type: 'Income' },
    { keywords: ['freelance', 'upwork', 'fiverr', 'consulting'], category: 'Freelance', type: 'Income' },
    { keywords: ['rent', 'lease', 'apartment'], category: currentMode === 'Office' ? 'Office Lease' : 'Housing', type: 'Expense' },
    { keywords: ['payroll', 'staff', 'employee', 'team'], category: 'Payroll', type: 'Expense' },
    { keywords: ['aws', 'azure', 'cloud', 'hosting', 'server', 'docker'], category: 'Cloud & Hosting', type: 'Expense' },
    { keywords: ['grocery', 'supermarket', 'mart', 'food shopping'], category: 'Groceries', type: 'Expense' },
    { keywords: ['coffee', 'lunch', 'dinner', 'restaurant', 'cafe', 'uber eats'], category: 'Food & Dining', type: 'Expense' },
    { keywords: ['internet', 'wifi', 'electricity', 'water', 'gas'], category: 'Utilities', type: 'Expense' },
    { keywords: ['marketing', 'ad', 'google ads', 'facebook'], category: 'Marketing', type: 'Expense' }
  ];

  // DOM Elements
  const modePersonalBtn = document.getElementById('modePersonal');
  const modeOfficeBtn = document.getElementById('modeOffice');
  const monthSelect = document.getElementById('monthSelect');
  const themeToggle = document.getElementById('themeToggle');
  const txForm = document.getElementById('txForm');
  const txTitleInput = document.getElementById('txTitle');
  const txTypeSelect = document.getElementById('txType');
  const txCategorySelect = document.getElementById('txCategory');
  const txAmountInput = document.getElementById('txAmount');
  const txDateInput = document.getElementById('txDate');
  const smartHint = document.getElementById('smartHint');
  const ledgerList = document.getElementById('ledgerList');
  const resetDemoBtn = document.getElementById('resetDemoBtn');
  const copyCliBtn = document.getElementById('copyCliBtn');
  const toastContainer = document.getElementById('toastContainer');

  // Set default date to today
  txDateInput.value = '2026-09-28';

  // Theme Management
  themeToggle.addEventListener('click', () => {
    const currentTheme = document.body.getAttribute('data-theme');
    const newTheme = currentTheme === 'light' ? 'dark' : 'light';
    document.body.setAttribute('data-theme', newTheme);
    localStorage.setItem('sft_theme', newTheme);
    showToast(`Switched to ${newTheme} mode`, 'info');
  });

  const savedTheme = localStorage.getItem('sft_theme');
  if (savedTheme) {
    document.body.setAttribute('data-theme', savedTheme);
  }

  // Toast Functionality
  function showToast(message, type = 'info') {
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    
    let icon = 'ℹ️';
    if (type === 'success') icon = '✅';
    if (type === 'danger') icon = '⚠️';

    toast.innerHTML = `<span>${icon}</span><span>${message}</span>`;
    toastContainer.appendChild(toast);

    setTimeout(() => {
      toast.style.opacity = '0';
      toast.style.transform = 'translateX(100%)';
      toast.style.transition = 'all 0.3s ease';
      setTimeout(() => toast.remove(), 300);
    }, 3500);
  }

  // Populate Categories
  function populateCategories() {
    const list = categories[currentMode];
    txCategorySelect.innerHTML = '';
    list.forEach(c => {
      const opt = document.createElement('option');
      opt.value = c.name;
      opt.textContent = `${c.icon} ${c.name}`;
      txCategorySelect.appendChild(opt);
    });
  }

  // Smart Keyword Categorizer
  txTitleInput.addEventListener('input', (e) => {
    const text = e.target.value.toLowerCase().trim();
    if (!text) {
      smartHint.textContent = '💡 Smart auto-categorization active';
      return;
    }

    for (const rule of smartRules) {
      if (rule.keywords.some(k => text.includes(k))) {
        // Find if category exists in current mode
        const match = categories[currentMode].find(c => c.name.toLowerCase() === rule.category.toLowerCase());
        if (match) {
          txCategorySelect.value = match.name;
          txTypeSelect.value = rule.type;
          smartHint.textContent = `✨ Auto-categorized as ${match.name} (${rule.type})`;
          return;
        }
      }
    }
    smartHint.textContent = '💡 Smart auto-categorization active';
  });

  // Mode Switcher Handlers
  modePersonalBtn.addEventListener('click', () => {
    if (currentMode === 'Personal') return;
    currentMode = 'Personal';
    modePersonalBtn.classList.add('active');
    modePersonalBtn.setAttribute('aria-selected', 'true');
    modeOfficeBtn.classList.remove('active');
    modeOfficeBtn.setAttribute('aria-selected', 'false');
    populateCategories();
    renderSimulator();
    showToast('Switched to Personal Pocket Finance Mode', 'info');
  });

  modeOfficeBtn.addEventListener('click', () => {
    if (currentMode === 'Office') return;
    currentMode = 'Office';
    modeOfficeBtn.classList.add('active');
    modeOfficeBtn.setAttribute('aria-selected', 'true');
    modePersonalBtn.classList.remove('active');
    modePersonalBtn.setAttribute('aria-selected', 'false');
    populateCategories();
    renderSimulator();
    showToast('Switched to Corporate Office Finance Mode', 'info');
  });

  // Month Switcher
  monthSelect.addEventListener('change', (e) => {
    currentMonth = e.target.value;
    renderSimulator();
    showToast(`Accounting ledger updated for period ${currentMonth}`, 'info');
  });

  // Add Transaction
  txForm.addEventListener('submit', (e) => {
    e.preventDefault();
    const title = txTitleInput.value.trim();
    const type = txTypeSelect.value;
    const category = txCategorySelect.value;
    const amount = parseFloat(txAmountInput.value);
    const date = txDateInput.value;

    if (!title || isNaN(amount) || amount <= 0) return;

    const newTx = {
      id: Date.now(),
      title,
      type,
      category,
      amount,
      date
    };

    const dataset = currentMode === 'Personal' ? personalTransactions : officeTransactions;
    dataset.unshift(newTx);

    txForm.reset();
    txDateInput.value = date;
    smartHint.textContent = '💡 Smart auto-categorization active';

    renderSimulator();
    showToast(`Transaction "${title}" added successfully!`, 'success');
  });

  // Render Simulator UI & Calculations
  function renderSimulator() {
    const dataset = currentMode === 'Personal' ? personalTransactions : officeTransactions;
    const filtered = dataset.filter(t => t.date.startsWith(currentMonth));

    // Calculate Inflow & Outflow
    let monthlyIncome = 0;
    let monthlyExpenses = 0;
    const categoryTotals = {};

    filtered.forEach(t => {
      if (t.type === 'Income') {
        monthlyIncome += t.amount;
      } else {
        monthlyExpenses += t.amount;
        categoryTotals[t.category] = (categoryTotals[t.category] || 0) + t.amount;
      }
    });

    const netPeriod = monthlyIncome - monthlyExpenses;

    // Update KPI Cards Based on Mode
    const kpiLabel1 = document.getElementById('kpiLabel1');
    const kpiVal1 = document.getElementById('kpiVal1');
    const kpiMeta1 = document.getElementById('kpiMeta1');
    const kpiIcon1 = document.getElementById('kpiIcon1');

    const kpiLabel2 = document.getElementById('kpiLabel2');
    const kpiVal2 = document.getElementById('kpiVal2');
    const kpiMeta2 = document.getElementById('kpiMeta2');

    const kpiLabel3 = document.getElementById('kpiLabel3');
    const kpiVal3 = document.getElementById('kpiVal3');
    const kpiMeta3 = document.getElementById('kpiMeta3');

    const kpiLabel4 = document.getElementById('kpiLabel4');
    const kpiVal4 = document.getElementById('kpiVal4');
    const kpiMeta4 = document.getElementById('kpiMeta4');
    const kpiIcon4 = document.getElementById('kpiIcon4');

    if (currentMode === 'Personal') {
      kpiLabel1.textContent = 'Total Balance';
      kpiIcon1.textContent = '💳';
      const totalBalance = 11070 + netPeriod;
      kpiVal1.textContent = `$${totalBalance.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
      kpiMeta1.textContent = 'Available liquid funds';

      kpiLabel2.textContent = 'Monthly Income';
      kpiVal2.textContent = `+$${monthlyIncome.toLocaleString('en-US', { minimumFractionDigits: 2 })}`;
      kpiMeta2.textContent = `${filtered.filter(t => t.type === 'Income').length} verified deposits`;

      kpiLabel3.textContent = 'Monthly Expenses';
      kpiVal3.textContent = `-$${monthlyExpenses.toLocaleString('en-US', { minimumFractionDigits: 2 })}`;
      const ratio = monthlyIncome > 0 ? ((monthlyExpenses / monthlyIncome) * 100).toFixed(1) : 0;
      kpiMeta3.textContent = `${ratio}% of monthly income`;

      kpiLabel4.textContent = 'Budget Health Score';
      kpiIcon4.textContent = '🛡️';
      const health = Math.max(10, Math.min(100, Math.round(100 - (ratio * 0.8))));
      kpiVal4.textContent = `${health} / 100`;
      kpiMeta4.textContent = health > 75 ? 'Optimal financial pacing' : 'Approaching budget limit';
    } else {
      // Office Finance Mode
      kpiLabel1.textContent = 'Corporate Treasury';
      kpiIcon1.textContent = '🏦';
      const corporateTreasury = 245000 + netPeriod;
      kpiVal1.textContent = `$${corporateTreasury.toLocaleString('en-US', { minimumFractionDigits: 2 })}`;
      kpiMeta1.textContent = 'Operating capital reserves';

      kpiLabel2.textContent = 'B2B Revenue';
      kpiVal2.textContent = `+$${monthlyIncome.toLocaleString('en-US', { minimumFractionDigits: 2 })}`;
      kpiMeta2.textContent = 'Client sales & subscriptions';

      kpiLabel3.textContent = 'Burn Rate (OPEX)';
      kpiVal3.textContent = `-$${monthlyExpenses.toLocaleString('en-US', { minimumFractionDigits: 2 })}`;
      kpiMeta3.textContent = 'Monthly operating expenditures';

      kpiLabel4.textContent = 'Liquid Runway';
      kpiIcon4.textContent = '🚀';
      const monthlyBurn = monthlyExpenses > 0 ? monthlyExpenses : 25000;
      const runwayMonths = (corporateTreasury / monthlyBurn).toFixed(1);
      kpiVal4.textContent = `${runwayMonths} Months`;
      kpiMeta4.textContent = 'At current net burn trajectory';
    }

    // Render Ledger List
    ledgerList.innerHTML = '';
    if (filtered.length === 0) {
      ledgerList.innerHTML = `<div class="ledger-item" style="justify-content: center; color: var(--text-muted);">No transactions recorded for ${currentMonth}</div>`;
    } else {
      filtered.forEach(t => {
        const catObj = categories[currentMode].find(c => c.name === t.category) || { icon: '💼', color: '#6b7280' };
        const item = document.createElement('div');
        item.className = 'ledger-item';
        item.innerHTML = `
          <div class="item-left">
            <div class="item-cat-icon" style="background: ${catObj.color}22; color: ${catObj.color};">
              ${catObj.icon}
            </div>
            <div class="item-details">
              <span class="item-title">${escapeHtml(t.title)}</span>
              <span class="item-sub">${t.category} • ${t.date}</span>
            </div>
          </div>
          <div class="item-right">
            <span class="item-amount ${t.type === 'Income' ? 'income' : 'expense'}">
              ${t.type === 'Income' ? '+' : '-'}$${t.amount.toLocaleString('en-US', { minimumFractionDigits: 2 })}
            </span>
            <button class="item-del-btn" data-id="${t.id}" title="Remove entry">✕</button>
          </div>
        `;
        ledgerList.appendChild(item);
      });
    }

    // Attach Delete Buttons
    document.querySelectorAll('.item-del-btn').forEach(btn => {
      btn.addEventListener('click', (e) => {
        const id = parseInt(e.currentTarget.getAttribute('data-id'));
        const dataset = currentMode === 'Personal' ? personalTransactions : officeTransactions;
        const idx = dataset.findIndex(t => t.id === id);
        if (idx !== -1) {
          const removed = dataset.splice(idx, 1)[0];
          renderSimulator();
          showToast(`Removed "${removed.title}"`, 'danger');
        }
      });
    });

    // Render Breakdown Bar & Legend
    renderBreakdown(categoryTotals, monthlyExpenses);
  }

  function renderBreakdown(categoryTotals, totalExpenses) {
    const breakdownBar = document.getElementById('breakdownBar');
    const breakdownLegend = document.getElementById('breakdownLegend');
    breakdownBar.innerHTML = '';
    breakdownLegend.innerHTML = '';

    if (totalExpenses <= 0) {
      breakdownBar.innerHTML = `<div class="bar-segment" style="width: 100%; background: #334155;"></div>`;
      breakdownLegend.innerHTML = `<span style="color: var(--text-muted);">No expense entries in this period</span>`;
      return;
    }

    Object.entries(categoryTotals).forEach(([catName, amount]) => {
      const catObj = categories[currentMode].find(c => c.name === catName) || { color: '#6b7280' };
      const pct = ((amount / totalExpenses) * 100).toFixed(1);

      // Segment
      const segment = document.createElement('div');
      segment.className = 'bar-segment';
      segment.style.width = `${pct}%`;
      segment.style.backgroundColor = catObj.color;
      segment.title = `${catName}: $${amount.toFixed(2)} (${pct}%)`;
      breakdownBar.appendChild(segment);

      // Legend Item
      const legend = document.createElement('div');
      legend.className = 'legend-item';
      legend.innerHTML = `
        <span class="legend-color" style="background: ${catObj.color};"></span>
        <span>${catName} (${pct}%)</span>
      `;
      breakdownLegend.appendChild(legend);
    });
  }

  function escapeHtml(str) {
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
  }

  // Reset Demo Data
  resetDemoBtn.addEventListener('click', () => {
    location.reload();
  });

  // Copy CLI Snippet
  copyCliBtn.addEventListener('click', () => {
    const code = `git clone https://github.com/rahator44/Smart_Finance_tracer.git\ncd Smart_Finance_tracer\ndotnet restore\ndotnet run --project SmartFinanceManager`;
    navigator.clipboard.writeText(code).then(() => {
      copyCliBtn.textContent = 'Copied!';
      showToast('CLI command copied to clipboard!', 'success');
      setTimeout(() => { copyCliBtn.textContent = 'Copy Command'; }, 2000);
    });
  });

  // Initialize
  populateCategories();
  renderSimulator();
});
