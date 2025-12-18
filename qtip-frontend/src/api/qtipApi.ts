const API_BASE_URL =
    import.meta.env.VITE_API_BASE_URL?.replace(/\/+$/, '') ?? 'http://localhost:5080';

export type StatsResponse = {
    totalPiiEmailCount: number;
};

export async function getPiiEmailStats(): Promise<number> {
    const res = await fetch(`${API_BASE_URL}/api/statistics/pii-email-count`);
    if (!res.ok) throw new Error(`Stats failed: ${res.status}`);
    return res.json();
}

export type SubmitResponse = {
    submissionId?: string | number;
    tokenisedText?: string;
    detectedCount?: number;
    totalPiiEmailCount?: number;
};

export async function submitText(text: string): Promise<SubmitResponse> {
    const res = await fetch(`${API_BASE_URL}/api/submissions`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ text }),
    });

    if (!res.ok) throw new Error(`Submit failed: ${res.status}`);
    return res.json();
}
