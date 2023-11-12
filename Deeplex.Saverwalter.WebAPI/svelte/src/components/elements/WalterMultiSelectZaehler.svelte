<script lang="ts">
    import type { WalterSelectionEntry } from "$walter/lib";
    import { walter_selection } from "$walter/services/requests";
    import WalterQuickAddButton from "./WalterQuickAddButton.svelte";
    import WalterMultiSelect from "./WalterMultiSelect.svelte";
    import { WalterZaehler } from "..";

    export let value: WalterSelectionEntry[] | undefined;
    export let titleText: string;
    export let fetchImpl: typeof fetch;
    export let readonly: boolean = false;
    
    let entries = walter_selection.zaehler(fetchImpl);
    function onSubmit()
    {
        entries = walter_selection.zaehler(fetchImpl);
    }

    let addEntry = {};
</script>

<WalterMultiSelect
    disabled={readonly}
    bind:value
    {titleText}
    {entries}/>

<WalterQuickAddButton
    bind:addEntry
    addUrl="/api/zaehler"
    {onSubmit}>
    <WalterZaehler entry={addEntry} {fetchImpl} />
</WalterQuickAddButton>
