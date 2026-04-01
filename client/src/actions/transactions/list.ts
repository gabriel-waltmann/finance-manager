  "use server";

export interface Transaction {
  id: string;
  date: string;
  title: string;
  amount: number;
  updated_at: string;
  created_at: string;
  deleted_at: string | null;
}

export type ListTransactionsResponse = {
  data?: Transaction[];
  error?: string;
};

export async function listTransactions({
  limit = 10,
  page = 1,
  startDate,
  endDate,
}: {
  limit?: number;
  page?: number;
  startDate?: string;
  endDate?: string;
} = {}): Promise<ListTransactionsResponse> {
  try {
    const url = new URL("http://localhost:8080/transactions");
    
    url.searchParams.append("limit", limit.toString());
    url.searchParams.append("page", page.toString());
    
    if (startDate) {
      url.searchParams.append("startDate", startDate);
    }
    
    if (endDate) {
      url.searchParams.append("endDate", endDate);
    }

    const response = await fetch(url.toString(), {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (!response.ok) {
      return { error: `Failed to fetch transactions. Status: ${response.status}` };
    }

    const data: Transaction[] = await response.json();
    return { data };
  } catch (error) {
    console.error("Error in listTransactions:", error);
    return { error: "An unexpected error occurred while fetching data." };
  }
}
