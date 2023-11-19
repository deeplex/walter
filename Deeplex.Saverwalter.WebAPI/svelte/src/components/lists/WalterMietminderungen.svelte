<script lang="ts">
    import { WalterDataWrapper, WalterMietminderung } from '$walter/components';
    import { WalterMietminderungEntry } from '$walter/lib';
    import { walter_goto } from '$walter/services/utils';

    const headers = [
        { key: 'beginn', value: 'Beginn' },
        { key: 'ende', value: 'Ende' },
        { key: 'minderung', value: 'Minderung' }
    ];

    export let rows: WalterMietminderungEntry[];
    export let fullHeight = false;
    export let title: string | undefined = undefined;

    const navigate = (e: CustomEvent) =>
        walter_goto(`/mietminderungen/${e.detail.id}`);

    export let entry: Partial<WalterMietminderungEntry> | undefined = undefined;
</script>

<WalterDataWrapper
    addUrl={WalterMietminderungEntry.ApiURL}
    {navigate}
    addEntry={entry}
    {title}
    {rows}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterMietminderung {entry} />
    {/if}
</WalterDataWrapper>
