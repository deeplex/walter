import { walter_get } from '$walter/services/requests';

export abstract class WalterApiHandler {
    protected static ApiURL: string;

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    public static fromJson(_json: unknown) {
        throw new Error('WalterApiHandler.fromJson not implemented.');
    }

    public static async GetAll<T extends WalterApiHandler>(
        fetchImpl: typeof fetch
    ): Promise<T[]> {
        const response = await walter_get(this.ApiURL, fetchImpl);

        if (!Array.isArray(response))
        {
            throw new Error("Expected response to be an array.");
        }

        return response.map<T>(json => this.fromJson(json) as unknown as T);
    }

    public static async GetOne<T extends WalterApiHandler>(
        id: string,
        fetchImpl: typeof fetch
    ): Promise<T> {
        const url = `${this.ApiURL}/${id}`;
        const response = await walter_get(url, fetchImpl);

        return this.fromJson(response) as unknown as T;
    }
}
