import { useEffect, useState } from "react";
import { api, ApiError } from "../api/client";
import type { Totals } from "../api/types";
import { formatCurrency } from "../utils/format";

interface Props {
  // Reloads whenever 'version' changes (person/transaction created or deleted).
  version: number;
}

// Totals section: per-person totals plus the overall totals.
export function TotalsSection({ version }: Props) {
  const [totals, setTotals] = useState<Totals | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api
      .getTotals()
      .then(setTotals)
      .catch((err) =>
        setError(err instanceof ApiError ? err.message : "Falha ao carregar totais.")
      );
  }, [version]);

  // CSS class based on the balance sign (positive/negative).
  const balanceClass = (v: number) => (v < 0 ? "value-negative" : "value-positive");

  return (
    <section className="card">
      <h2>Totais</h2>

      {error && <p className="error-box">{error}</p>}

      {!totals || totals.people.length === 0 ? (
        <p className="empty-state">Sem dados para exibir.</p>
      ) : (
        <table className="data-table">
          <thead>
            <tr>
              <th>Pessoa</th>
              <th className="col-amount">Receitas</th>
              <th className="col-amount">Despesas</th>
              <th className="col-amount">Saldo</th>
            </tr>
          </thead>
          <tbody>
            {totals.people.map((p) => (
              <tr key={p.personId}>
                <td>{p.name}</td>
                <td className="col-amount value-positive">{formatCurrency(p.totalIncome)}</td>
                <td className="col-amount value-negative">{formatCurrency(p.totalExpenses)}</td>
                <td className={`col-amount ${balanceClass(p.balance)}`}>{formatCurrency(p.balance)}</td>
              </tr>
            ))}
          </tbody>
          {/* Footer with the overall consolidated totals */}
          <tfoot>
            <tr>
              <th>Total geral</th>
              <th className="col-amount value-positive">
                {formatCurrency(totals.overallTotalIncome)}
              </th>
              <th className="col-amount value-negative">
                {formatCurrency(totals.overallTotalExpenses)}
              </th>
              <th className={`col-amount ${balanceClass(totals.overallBalance)}`}>
                {formatCurrency(totals.overallBalance)}
              </th>
            </tr>
          </tfoot>
        </table>
      )}
    </section>
  );
}
