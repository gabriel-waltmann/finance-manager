"use client";

import { useEffect, useState, useRef, useCallback } from "react";
import { listTransactions, Transaction } from "@/actions/transactions/list";

export function TransactionsTable({
  initialTransactions,
  startDate,
  endDate,
}: {
  initialTransactions: Transaction[];
  startDate: string;
  endDate: string;
}) {
  const [transactions, setTransactions] = useState<Transaction[]>(initialTransactions);
  const [page, setPage] = useState(1);
  const [hasMore, setHasMore] = useState(initialTransactions.length === 10);
  const [loading, setLoading] = useState(false);

  const observerRef = useRef<IntersectionObserver | null>(null);
  
  const loadMoreElementRef = useCallback(
    (node: HTMLDivElement | null) => {
      if (loading || !hasMore) return;
      if (observerRef.current) observerRef.current.disconnect();

      observerRef.current = new IntersectionObserver((entries) => {
        if (entries[0].isIntersecting) {
          loadMoreTransactions();
        }
      });

      if (node) observerRef.current.observe(node);
    },
    [loading, hasMore]
  );

  const loadMoreTransactions = async () => {
    setLoading(true);
    const nextPage = page + 1;
    const { data, error } = await listTransactions({ 
      page: nextPage, 
      limit: 10,
      startDate,
      endDate
    });
    
    if (data && data.length > 0) {
      setTransactions((prev) => [...prev, ...data]);
      setPage(nextPage);
      if (data.length < 10) {
        setHasMore(false);
      }
    } else {
      setHasMore(false);
    }
    
    setLoading(false);
  };

  if (!transactions || transactions.length === 0) {
    return <div className="text-gray-500 italic mt-4">No transactions found.</div>;
  }

  return (
    <>
      <table className="min-w-full text-left border-collapse">
        <thead>
          <tr className="border-b-2 border-[#121212]">
            <th className="py-3 px-4 font-semibold text-[#121212]">Date</th>
            <th className="py-3 px-4 font-semibold text-[#121212]">Title</th>
            <th className="py-3 px-4 font-semibold text-[#121212] text-right">Amount</th>
          </tr>
        </thead>
        <tbody>
          {transactions.map((item) => (
            <tr key={item.id} className="border-b border-gray-200 hover:bg-zinc-100 transition-colors">
              <td className="py-3 px-4 text-[#121212]">
                {new Date(item.date).toLocaleDateString("pt-br", {
                  day: "2-digit",
                  month: "2-digit",
                  year: "numeric",
                })}
              </td>
              <td className="py-3 px-4 text-[#121212]">{item.title}</td>
              <td className="py-3 px-4 text-[#121212] text-right">
                R${item.amount.toFixed(2)}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      
      {hasMore && (
        <div ref={loadMoreElementRef} className="py-4 text-center text-sm text-gray-500 flex justify-center">
          {loading ? "Loading more..." : "Scroll to load more"}
        </div>
      )}
    </>
  );
}