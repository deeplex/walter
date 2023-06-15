<script lang="ts">
    import { goto } from '$app/navigation';
    import { WalterDataWrapper, WalterMietminderung } from '$walter/components';
    import type { WalterMietminderungEntry } from '$walter/lib';

    const headers = [
        { key: 'beginn', value: 'Beginn' },
        { key: 'ende', value: 'Ende' },
        { key: 'minderung', value: 'Minderung' }
    ];

    const addUrl = `/api/mietminderungen/`;

    export let rows: WalterMietminderungEntry[];
    export let search = false;
    export let title: string | undefined = undefined;

    const navigate = (e: CustomEvent) =>
        goto(`/mietminderungen/${e.detail.id}`);

    export let entry: Partial<WalterMietminderungEntry> | undefined = undefined;
</script>

<WalterDataWrapper
    {navigate}
    {addUrl}
    addEntry={entry}
    {title}
    {search}
    {rows}
    {headers}
>
    {#if entry}
        <WalterMietminderung {entry} />
    {/if}
</WalterDataWrapper>
