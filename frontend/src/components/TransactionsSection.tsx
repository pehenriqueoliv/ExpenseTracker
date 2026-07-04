import { useEffect, useState } from "react";
import { api, ApiError } from "../api/client";
import type { Person, Transaction, TransactionType } from "../api/types";
import { formatCurrency, transactionTypeLabel } from "../utils/format";

interface Props {
  people: Person[];
  // 'version' is a counter that changes whenever something relevant changes in
  // App; it is used as a useEffect dependency to reload the list.
  version: number;
  onChanged: () => Promise<void> | void;
}

// Transaction registration + listing section.
export function TransactionsSection({ people, version, onChanged }: Props) {
  const [transactions, setTransactions] = useState<Transaction[]>([]);

  // Form fields.
  const [description, setDescription] = useState("");
  const [amount, setAmount] = useState("");
  const [type, setType] = useState<TransactionType>("Expense");
  const [personId, setPersonId] = useState("");

  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  // Reload the transactions on mount and whenever 'version' changes (e.g. after
  // creating a transaction or deleting a person with cascade).
  useEffect(() => {
    api
      .listTransactions()
      .then(setTransactions)
      .catch((err) =>
        setError(err instanceof ApiError ? err.message : "Falha ao carregar transações.")
      );
  }, [version]);

  // id -> name map to show the person's name in the transaction list.
  const nameById = new Map(people.map((p) => [p.id, p.name]));

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setSaving(true);
    try {
      await api.createTransaction({
        description,
        amount: Number(amount),
        type,
        personId,
      });
      // Clear only the value/description fields; keep person and type selected.
      setDescription("");
      setAmount("");
      await onChanged();
    } catch (err) {
      // Business-rule messages from the API surface here, e.g.:
      // "A pessoa 'Joao' é menor de 18 anos e só pode cadastrar Despesa." (400)
      // or "Pessoa ... não encontrada." (404)
      setError(err instanceof ApiError ? err.message : "Falha ao criar transação.");
    } finally {
      setSaving(false);
    }
  }

  const noPeople = people.length === 0;

  return (
    <section className="card">
      <h2>Transações</h2>

      {noPeople ? (
        <p className="empty-state">Cadastre uma pessoa antes de lançar transações.</p>
      ) : (
        <form className="form form-transaction" onSubmit={handleSubmit}>
          <div className="field">
            <label htmlFor="tr-description">Descrição</label>
            <input
              id="tr-description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Ex.: Salário"
              required
            />
          </div>

          <div className="field field-amount">
            <label htmlFor="tr-amount">Valor</label>
            <input
              id="tr-amount"
              type="number"
              step="0.01"
              min="0.01"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              placeholder="0,00"
              required
            />
          </div>

          {/* Type dropdown (enum Expense | Income), labeled in Portuguese */}
          <div className="field field-type">
            <label htmlFor="tr-type">Tipo</label>
            <select
              id="tr-type"
              value={type}
              onChange={(e) => setType(e.target.value as TransactionType)}
            >
              <option value="Expense">Despesa</option>
              <option value="Income">Receita</option>
            </select>
          </div>

          {/* Person dropdown (the PersonId foreign key) */}
          <div className="field">
            <label htmlFor="tr-person">Pessoa</label>
            <select
              id="tr-person"
              value={personId}
              onChange={(e) => setPersonId(e.target.value)}
              required
            >
              <option value="" disabled>
                Selecione...
              </option>
              {people.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.name} ({p.age} anos)
                </option>
              ))}
            </select>
          </div>

          <button type="submit" disabled={saving}>
            {saving ? "Salvando..." : "Adicionar"}
          </button>
        </form>
      )}

      {error && <p className="error-box">{error}</p>}

      {transactions.length === 0 ? (
        <p className="empty-state">Nenhuma transação lançada ainda.</p>
      ) : (
        <table className="data-table">
          <thead>
            <tr>
              <th>Descrição</th>
              <th>Pessoa</th>
              <th>Tipo</th>
              <th className="col-amount">Valor</th>
            </tr>
          </thead>
          <tbody>
            {transactions.map((t) => (
              <tr key={t.id}>
                <td>{t.description}</td>
                <td>{nameById.get(t.personId) ?? "—"}</td>
                <td>
                  <span className={t.type === "Income" ? "tag tag-income" : "tag tag-expense"}>
                    {transactionTypeLabel(t.type)}
                  </span>
                </td>
                <td className="col-amount">{formatCurrency(t.amount)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  );
}
