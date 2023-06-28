<script lang="ts">
    import { WalterNumberInput } from '$walter/components';
    import { WalterToastContent } from '$walter/lib';
    import {
        create_abrechnung_pdf,
        create_abrechnung_word,
        loadAbrechnung
    } from '$walter/services/abrechnung';
    import type {
        WalterBetriebskostenabrechnungKostengruppenEntry,
        WalterS3File
    } from '$walter/types';
    import { goto } from '$app/navigation';
    import { page } from '$app/stores';

    import { Button, ButtonSet, Row } from 'carbon-components-svelte';
    import { create_pdf_doc, create_word_doc } from './WalterAbrechnungControl';

    export let firstYear: number;

    export let vertragId: string;
    export let fetchImpl: typeof fetch;
    export let S3URL: string;
    export let title: string;
    export let S3files: WalterS3File[];
    export let abrechnung: WalterBetriebskostenabrechnungKostengruppenEntry;

    let searchParams: URLSearchParams = new URL($page.url).searchParams;
    let selectedYear =
        +(searchParams.get('abrechnung') || 0) ||
        Math.max(new Date().getFullYear() - 1, firstYear);

    // TODO rework -> inject fileWrapper instead
    function addToAnhang(file: WalterS3File) {
        if (S3files.some((e) => e.FileName == file.FileName)) {
            return;
        }
        S3files = [...S3files, file];
    }

    async function vorschau_erstellen_click(): Promise<void> {
        searchParams = new URLSearchParams({ abrechnung: `${selectedYear}` });
        goto(`?${searchParams.toString()}`, { noScroll: true });
        abrechnung = await loadAbrechnung(
            vertragId,
            `${selectedYear}`,
            fetchImpl
        );
    }

    async function click_word(e: MouseEvent): Promise<void> {
        const file = await create_word_doc(
            vertragId,
            selectedYear,
            title,
            S3URL,
            fetchImpl
        );

        if (file) {
            addToAnhang(file);
        }
    }

    async function click_pdf(e: MouseEvent): Promise<void> {
        const file = await create_pdf_doc(
            vertragId,
            selectedYear,
            title,
            S3URL,
            fetchImpl
        );

        if (file) {
            addToAnhang(file);
        }
    }
</script>

<Row>
    <WalterNumberInput
        min={firstYear}
        bind:value={selectedYear}
        label="Jahr"
        hideSteppers={false}
    />
    <ButtonSet style="margin: auto">
        <Button on:click={vorschau_erstellen_click}>Vorschau anzeigen</Button>
        <Button on:click={click_word}>Word-Dokument erstellen</Button>
        <Button on:click={click_pdf}>PDF-Dokument erstellen</Button>
    </ButtonSet>
</Row>
