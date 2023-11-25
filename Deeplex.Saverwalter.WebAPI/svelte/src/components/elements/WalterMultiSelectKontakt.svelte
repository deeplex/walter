<script lang="ts">
    import {
        WalterKontaktEntry,
        WalterSelectionEntry
    } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';
    import WalterQuickAddButton from './WalterQuickAddButton.svelte';
    import WalterMultiSelect from './WalterMultiSelect.svelte';
    import WalterKontakt from '../details/WalterKontakt.svelte';

    export let value: WalterSelectionEntry[] | undefined;
    export let titleText: string;
    export let fetchImpl: typeof fetch;
    export let readonly: boolean = false;

    let entries = walter_selection.kontakte(fetchImpl);
    function onSubmit() {
        entries = walter_selection.kontakte(fetchImpl);
    }

    let addEntry = {};
</script>

<div
    style="
    flex: 1 1 auto !important;
    display: flex !important;
    flex-wrap: wrap !important"
>
    <WalterMultiSelect disabled={readonly} bind:value {titleText} {entries} />

    <WalterQuickAddButton
        bind:addEntry
        addUrl={WalterKontaktEntry.ApiURL}
        {onSubmit}
    >
        <WalterKontakt entry={addEntry} {fetchImpl} />
    </WalterQuickAddButton>
</div>
