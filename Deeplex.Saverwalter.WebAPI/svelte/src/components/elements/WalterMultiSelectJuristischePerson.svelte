<script lang="ts">
    import type { WalterSelectionEntry } from "$walter/lib";
    import { walter_selection } from "$walter/services/requests";
    import WalterQuickAddButton from "./WalterQuickAddButton.svelte";
    import WalterMultiSelect from "./WalterMultiSelect.svelte";
    import { WalterJuristischePerson } from "..";

    export let value: WalterSelectionEntry[] | undefined;
    export let titleText: string;
    export let fetchImpl: typeof fetch;
    export let readonly: boolean = false;
    
    let entries = walter_selection.juristischePersonen(fetchImpl);
    function onSubmit()
    {
        entries = walter_selection.juristischePersonen(fetchImpl);
    }

    let addEntry = {};
</script>

<div style="
    flex: 1 1 auto !important;
    display: flex !important;
    flex-wrap: wrap !important">
    <WalterMultiSelect
    disabled={readonly}
    bind:value
    {titleText}
    {entries}/>

<WalterQuickAddButton
    title="Juristische Personen"
    bind:addEntry
    addUrl="/api/kontakte/jur"
    {onSubmit}>
    <WalterJuristischePerson entry={addEntry} {fetchImpl} />
</WalterQuickAddButton>
</div>
