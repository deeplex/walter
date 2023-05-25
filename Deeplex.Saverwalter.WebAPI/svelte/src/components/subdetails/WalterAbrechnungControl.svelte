<script lang="ts">
    import { WalterNumberInput } from '$WalterComponents';
    import { WalterToastContent } from '$WalterLib';
    import {
        create_abrechnung_pdf,
        create_abrechnung_word,
        loadAbrechnung
    } from '$WalterServices/abrechnung';
    import {
        create_walter_s3_file_from_file,
        walter_s3_post
    } from '$WalterServices/s3';
    import type {
        WalterBetriebskostenabrechnungKostengruppenEntry,
        WalterS3File
    } from '$WalterTypes';
    import { goto } from '$app/navigation';
    import { page } from '$app/stores';

    import { Button, ButtonSet, Row } from 'carbon-components-svelte';

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

    async function word_dokument_erstellen_click() {
        const abrechnung = await create_abrechnung_word(
            vertragId,
            selectedYear,
            title
        );
        if (abrechnung instanceof File) {
            create_abrechnung(abrechnung);
        }
    }

    async function pdf_dokument_erstellen_click() {
        const abrechnung = await create_abrechnung_pdf(
            vertragId,
            selectedYear,
            title
        );
        if (abrechnung instanceof File) {
            create_abrechnung(abrechnung);
        }
    }

    async function create_abrechnung(abrechnung: File) {
        const file = create_walter_s3_file_from_file(abrechnung, S3URL);

        const toast = new WalterToastContent(
            'Hochladen erfolgreich',
            'Hochladen fehlgeschlagen',
            () => `Die Datei: ${file.FileName} wurde erfolgreich hochgeladen`,
            () => `Die Datei: ${file.FileName} konnte nicht hochgeladen werden.`
        );

        var response = await walter_s3_post(
            new File([abrechnung], file.FileName),
            S3URL,
            fetchImpl,
            toast
        );

        if (response.ok) {
            addToAnhang(file);
        }
    }

    function addToAnhang(file: WalterS3File) {
        if (S3files.some((e) => e.FileName == file.FileName)) {
            return;
        }
        S3files = [...S3files, file];
    }

    async function abrechnung_click() {
        searchParams = new URLSearchParams({ abrechnung: `${selectedYear}` });
        goto(`?${searchParams.toString()}`, { noScroll: true });
        abrechnung = await loadAbrechnung(
            vertragId,
            `${selectedYear}`,
            fetchImpl
        );
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
        <Button on:click={abrechnung_click}>Vorschau anzeigen</Button>
        <Button on:click={word_dokument_erstellen_click}
            >Word-Dokument erstellen</Button
        >
        <Button on:click={pdf_dokument_erstellen_click}
            >PDF-Dokument erstellen</Button
        >
    </ButtonSet>
</Row>
