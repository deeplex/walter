import { walter_get } from '$WalterServices/requests';

export abstract class WalterApiHandler {
    protected static ApiURL: string;

    public static fromJson(_json: unknown) {
        throw new Error('WalterApiHandler.fromJson not implemented.');
    }

    public static async GetAll<T extends WalterApiHandler>(
        fetch: any
    ): Promise<T[]> {
        const response = await walter_get(this.ApiURL, fetch);

        return response.map(this.fromJson);
    }

    public static async GetOne<T extends WalterApiHandler>(
        id: string,
        fetch: any
    ): Promise<T> {
        const url = `${this.ApiURL}/${id}`;
        const response = await walter_get(url, fetch);

        return this.fromJson(response) as unknown as T;
    }
}
