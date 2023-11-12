<script lang="ts">
    import type { WalterSelectionEntry } from "$walter/lib";
    import { walter_selection } from "$walter/services/requests";
    import { ContentSwitcher, Switch } from "carbon-components-svelte";
    import { WalterComboBox, WalterJuristischePerson, WalterNatuerlichePerson, WalterWohnung } from "..";
    import WalterQuickAddButton from "./WalterQuickAddButton.svelte";

    export let readonly: boolean = false;
    export let required: boolean = false;
    export let value: WalterSelectionEntry | undefined;
    export let fetchImpl: typeof fetch;
    export let title: string | undefined = undefined;
    
    let entries = walter_selection.kontakte(fetchImpl);
    function onSubmit()
    {
        entries = walter_selection.kontakte(fetchImpl);
    }

    let addEntry = {};

    let personType = 0;
</script>

<div style="
    flex: 1 1 auto !important;
    display: flex !important;
    flex-wrap: wrap !important">
<WalterComboBox
    {required}
    {readonly}
    bind:value
    titleText="{title || "Person"}"
    {entries}/>

<WalterQuickAddButton
    title="Personen"
    bind:addEntry
    addUrl="/api/kontakte/{personType ? "jur" : "nat"}"
    {onSubmit}>
    <ContentSwitcher
        bind:selectedIndex={personType}>
        <Switch text="Natürliche Person" />
        <Switch text="Juristische Person" />
    </ContentSwitcher>
    {#if personType === 0}
        <WalterNatuerlichePerson entry={addEntry} {fetchImpl} />
    {:else}
        <WalterJuristischePerson entry={addEntry} {fetchImpl} />
    {/if}
</WalterQuickAddButton>
</div>