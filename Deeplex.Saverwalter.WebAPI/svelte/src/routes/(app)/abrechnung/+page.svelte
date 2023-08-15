<script lang="ts">
    import { WalterAbrechnung, WalterGrid, WalterHeader, WalterNumberInput } from "$walter/components";
    import { onMount } from "svelte";
    import type { PageData } from "./$types";
    import type { WalterBetriebskostenabrechnungEntry } from "$walter/types";
    import { page } from "$app/stores";
    import { ComboBox, InlineNotification, Link, Loading, NumberInput, OverflowMenu, OverflowMenuItem, Row } from "carbon-components-svelte";
    import { shouldFilterItem } from "$walter/components/elements/WalterComboBox";
    import { create_pdf_doc, create_word_doc, updatePreview } from "./utils";
    import { Download } from "carbon-icons-svelte";
    import type { WalterSelectionEntry } from "$walter/lib";
    import { goto } from "$app/navigation";
    import { download } from "$walter/components/preview/WalterPreview";

    export let vertragId: number | null;
    export let selectedYear: number;

    let abrechnung: Promise<WalterBetriebskostenabrechnungEntry | undefined>;
    let title: string;
    const searchParams: URLSearchParams = new URL($page.url).searchParams;

    export let data: PageData;

    const likelyYear = new Date().getFullYear() - 1;

    let value: WalterSelectionEntry | undefined;

    async function update() {
        goto(`?${searchParams.toString()}`, { noScroll: true });
        abrechnung = updatePreview(vertragId, selectedYear, data.fetchImpl);

        const value = data.vertraege.find(vertrag => vertrag.id == vertragId);
        title = value?.text || "Wähle einen Vertrag aus";
    }

    onMount(async () => {
        vertragId = +(searchParams.get('vertrag') || 0) || null;
        selectedYear = +(searchParams.get('jahr') || 0) || likelyYear;
        value = data.vertraege.find(vertrag => vertrag.id == vertragId);

        searchParams.set("jahr", `${selectedYear}`);

        update();
    });

    function select(e: CustomEvent) {
        vertragId = e.detail.selectedItem?.id || vertragId;
        if (vertragId)
        {
            searchParams.set("vertrag", `${vertragId}`);
        }
        update();
    }

    function change(e: CustomEvent<number | null>)
    {
        selectedYear = e.detail || likelyYear;
        searchParams.set("jahr", `${selectedYear}`);
        update();
    }

    async function click_word(e: MouseEvent): Promise<void> {
        if (!vertragId || !selectedYear) return;

        const file = await create_word_doc(
            vertragId,
            selectedYear,
            title,
            data.fetchImpl
        );

        if (file)
        {
            download(file)
        }
    }

    async function click_pdf(e: MouseEvent): Promise<void> {
        if (!vertragId || !selectedYear) return;

        const file = await create_pdf_doc(
            vertragId,
            selectedYear,
            title,
            data.fetchImpl
        );

        if (file)
        {
            download(file)
        }
    }
</script>

<WalterHeader {title} />

<WalterGrid>
    <Row>
        <ComboBox
            placeholder="Wähle einen Vertrag aus..."
            selectedId={vertragId}
            on:select={select}
            items={data.vertraege}
            value={value?.text}
            titleText={title}
            {shouldFilterItem}
        />

        <div style="max-width: 15em">
        <NumberInput
            disabled={vertragId === null}
            on:change={change}
            bind:value={selectedYear}
            label="Jahr"
            hideSteppers={false}
        />
        </div>

        <OverflowMenu style="margin: auto; margin-bottom: 10px" flipped icon={Download}>
            <OverflowMenuItem on:click={click_word}>Word-Dokument erstellen</OverflowMenuItem>
            <OverflowMenuItem on:click={click_pdf}>PDF-Dokument erstellen</OverflowMenuItem>
        </OverflowMenu>
    </Row>

    {#await abrechnung}
        <Loading />
    {:then resolved}
        {#if resolved}
            {#if resolved.zeitraum.nutzungszeitraum > 0}
                <WalterAbrechnung
                    fetchImpl={data.fetchImpl}
                    abrechnung={resolved}
                    title={title}/>
            {:else}
                <InlineNotification lowContrast kind="error" hideCloseButton title="Abrechnungsjahr liegt außerhalb der Vertragslaufzeit.">
                    <Link href={`vertraege/${vertragId}`}>Klicke hier um zum Vertrag zu gelangen.</Link>
                </InlineNotification>
            {/if}
        {:else if vertragId}
            <InlineNotification lowContrast kind="error" hideCloseButton>
                Ups, da ist wohl irgendwas schiefgelaufen. Versuche die Seite neu zu laden...
            </InlineNotification>
        {/if}
    {/await}
</WalterGrid>