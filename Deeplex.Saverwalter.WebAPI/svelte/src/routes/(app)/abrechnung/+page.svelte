<!-- Copyright (C) 2023-2025  Kai Lawrence -->
<!--
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
-->

<script lang="ts">
    import {
        WalterAbrechnung,
        WalterAnhaenge,
        WalterGrid,
        WalterHeader,
        WalterLink
    } from '$walter/components';
    import { onMount } from 'svelte';
    import type { PageData } from './$types';
    import type {
        WalterBetriebskostenabrechnungEntry,
        WalterFile,
        WalterModalControl
    } from '$walter/types';
    import { page } from '$app/stores';
    import {
        Checkbox,
        ComboBox,
        HeaderAction,
        HeaderUtilities,
        InlineNotification,
        Loading,
        Modal,
        NumberInput,
        OverflowMenu,
        OverflowMenuItem,
        Row
    } from 'carbon-components-svelte';
    import { shouldFilterItem } from '$walter/components/elements/WalterComboBox';
    import { create_pdf_doc, create_word_doc, updatePreview } from './utils';
    import { Download } from 'carbon-icons-svelte';
    import { WalterFileWrapper, type WalterSelectionEntry } from '$walter/lib';
    import { download } from '$walter/components/preview/WalterPreview';
    import { walter_goto } from '$walter/services/utils';
    import { fileURL } from '$walter/services/files';
    import { WalterVertragEntry } from '$walter/lib/WalterVertrag';

    export let vertragId: number | null;
    export let selectedYear: number;

    let abrechnung: Promise<WalterBetriebskostenabrechnungEntry | undefined>;
    let resolvedAbrechnung: WalterBetriebskostenabrechnungEntry | undefined;
    let abrechnungLoading = false;
    let abrechnungError = false;
    let title: string;
    const searchParams: URLSearchParams = new URL($page.url).searchParams;

    export let data: PageData;

    const likelyYear = new Date().getFullYear() - 1;

    let value: WalterSelectionEntry | undefined;
    let availableYears: number[] = [];
    let canCreateMiete = false;
    let canReadMiete = false;

    let modalControl: WalterModalControl = {
        open: false,
        modalHeading: 'Abrechnung existiert bereits',
        content:
            'Es existiert bereits ein Ergebnis für die Abrechnung von diesem Vertrag in diesem Jahr. Möchtest du sie überschreiben?',
        danger: false,
        primaryButtonText: 'Bestätigen',
        submit: () => undefined
    };

    let fileWrapper = new WalterFileWrapper(data.fetchImpl);
    fileWrapper.registerStack();
    let files = '(0)';

    async function updateContractContext() {
        availableYears = [];
        canCreateMiete = false;
        canReadMiete = false;

        if (!vertragId) return;

        try {
            const vertrag = await WalterVertragEntry.GetOne<WalterVertragEntry>(
                `${vertragId}`,
                data.fetchImpl
            );

            canCreateMiete = !!vertrag.permissions?.update;
            canReadMiete =
                !!vertrag.permissions?.read || !!vertrag.permissions?.update;

            const start = new Date(vertrag.beginn).getFullYear();
            const end = vertrag.ende
                ? new Date(vertrag.ende).getFullYear()
                : likelyYear;

            if (!Number.isNaN(start) && !Number.isNaN(end)) {
                const min = Math.min(start, end);
                const max = Math.max(start, end);
                availableYears = Array.from(
                    { length: max - min + 1 },
                    (_, i) => min + i
                );
            }
        } catch {
            // Keep defaults if contract details cannot be loaded.
            availableYears = [];
            canCreateMiete = false;
            canReadMiete = false;
        }
    }

    async function update() {
        if (vertragId) {
            searchParams.set('vertrag', `${vertragId}`);
        } else {
            searchParams.delete('vertrag');
        }
        searchParams.set('jahr', `${selectedYear}`);

        walter_goto(`?${searchParams.toString()}`, { noScroll: true });
        abrechnungLoading = true;
        abrechnungError = false;
        updateContractContext();

        const pendingAbrechnung = updatePreview(
            vertragId,
            selectedYear,
            data.fetchImpl
        );
        abrechnung = pendingAbrechnung;

        const value = data.vertraege.find((vertrag) => vertrag.id == vertragId);
        title = value?.text || 'Wähle einen Vertrag aus';

        pendingAbrechnung
            .then(async (entry) => {
                if (abrechnung !== pendingAbrechnung) return;
                resolvedAbrechnung = entry;
                fileWrapper.clear();
                fileWrapper.registerStack();

                if (entry?.resultat) {
                    const furl = fileURL.abrechnungsresultat(
                        `${entry.resultat.id}`
                    );
                    fileWrapper.register('Resultat', furl);
                    files = `(${
                        (
                            await fileWrapper.handles.find(
                                (h) => h.fileURL == furl
                            )?.files
                        )?.length || 0
                    })`;
                }

                if (entry?.vertrag) {
                    fileWrapper.register(
                        'Vertrag',
                        fileURL.vertrag(`${entry.vertrag.id}`)
                    );
                }
            })
            .catch(() => {
                if (abrechnung !== pendingAbrechnung) return;
                resolvedAbrechnung = undefined;
                abrechnungError = true;
            });

        pendingAbrechnung.finally(() => {
            if (abrechnung === pendingAbrechnung) {
                abrechnungLoading = false;
            }
        });
    }

    onMount(async () => {
        vertragId = +(searchParams.get('vertrag') || 0) || null;
        selectedYear = +(searchParams.get('jahr') || 0) || likelyYear;
        value = data.vertraege.find((vertrag) => vertrag.id == vertragId);

        update();
    });

    function select(e: CustomEvent) {
        vertragId = e.detail.selectedItem?.id || vertragId;
        update();
    }

    function change(e: CustomEvent<number | null>) {
        selectedYear = e.detail || likelyYear;
        update();
    }

    function selectSuggestedYear(year: number) {
        selectedYear = year;
        update();
    }

    async function create_file(
        submitFn: () => Promise<WalterFile | undefined>
    ): Promise<WalterFile | undefined> {
        let file;
        if (!vertragId || !selectedYear) return undefined;
        if ((await abrechnung)?.resultat) {
            modalControl.open = true;
            modalControl.submit = async () => {
                file = await submitFn();
                if (file) {
                    update();
                    download(file);
                }
            };
        } else {
            override_active = true;
            file = await submitFn();
            if (file) {
                update();
                download(file);
            }
        }

        return file;
    }

    let override_active = false;

    async function click_word(): Promise<void> {
        let submit = async () => {
            if (!vertragId || !selectedYear) return;
            let override = override_active;
            override_active = false;
            return await create_word_doc(
                vertragId,
                selectedYear,
                title,
                override,
                data.fetchImpl
            );
        };

        await create_file(submit);
    }

    async function click_pdf(): Promise<void> {
        if (!vertragId || !selectedYear) return;

        let submit = async () => {
            if (!vertragId || !selectedYear) return;
            let override = override_active;
            override_active = false;
            return await create_pdf_doc(
                vertragId,
                selectedYear,
                title,
                override,
                data.fetchImpl
            );
        };

        await create_file(submit);
    }
</script>

<WalterHeader {title}>
    <HeaderUtilities>
        <HeaderAction isOpen preventCloseOnClickOutside bind:text={files}>
            <WalterAnhaenge bind:fileWrapper />
        </HeaderAction>
    </HeaderUtilities>
</WalterHeader>

<Modal
    {...modalControl}
    bind:open={modalControl.open}
    secondaryButtonText="Abbrechen"
    on:click:button--secondary={() => (modalControl.open = false)}
    on:click:button--primary={() => (modalControl.open = false)}
    on:open
    on:close
    on:submit={modalControl.submit}
>
    <p>{modalControl.content}</p>
    <Checkbox
        labelText="Abrechnungsergebnis überschreiben"
        bind:checked={override_active}
        style="margin-top: 1em"
    />
</Modal>

<WalterGrid>
    <Row>
        <ComboBox
            placeholder="Wähle einen Vertrag aus..."
            selectedId={vertragId}
            on:select={select}
            items={data.vertraege}
            value={value?.text}
            titleText="Vertrag"
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

        <OverflowMenu
            style="margin: auto; margin-bottom: 10px"
            flipped
            icon={Download}
        >
            <OverflowMenuItem on:click={click_word}
                >Word-Dokument erstellen</OverflowMenuItem
            >
            <OverflowMenuItem on:click={click_pdf}
                >PDF-Dokument erstellen</OverflowMenuItem
            >
        </OverflowMenu>
    </Row>

    {#if abrechnungLoading}
        <Loading withOverlay={false} />
    {/if}

    {#if resolvedAbrechnung && resolvedAbrechnung.zeitraum}
        {#if resolvedAbrechnung.zeitraum.nutzungszeitraum > 0}
            {#key `${resolvedAbrechnung.vertrag.id}-${resolvedAbrechnung.zeitraum.jahr}`}
                <WalterAbrechnung
                    fetchImpl={data.fetchImpl}
                    abrechnung={resolvedAbrechnung}
                    {title}
                    {canCreateMiete}
                    {canReadMiete}
                />
            {/key}
        {:else}
            <InlineNotification
                lowContrast
                kind="error"
                hideCloseButton
                title="Abrechnungsjahr liegt außerhalb der Vertragslaufzeit."
            >
                <p>
                    <WalterLink href={`/vertraege/${vertragId}`}
                        >Klicke hier um zum Vertrag zu gelangen.</WalterLink
                    >
                </p>
                {#if availableYears.length > 0}
                    <p style="margin-top: 0.5rem">
                        Mögliche Jahre:
                        {#each availableYears as year, i}
                            <!-- svelte-ignore a11y-invalid-attribute -->
                            <a
                                href="#"
                                on:click|preventDefault={() =>
                                    selectSuggestedYear(year)}>{year}</a
                            >{#if i < availableYears.length - 1},
                            {/if}
                        {/each}
                    </p>
                {/if}
            </InlineNotification>
        {/if}
    {:else if vertragId}
        {#if abrechnungLoading}
            <InlineNotification lowContrast kind="info" hideCloseButton>
                Lade Abrechnung...
            </InlineNotification>
        {:else if abrechnungError}
            <InlineNotification lowContrast kind="error" hideCloseButton>
                Abrechnung konnte nicht geladen werden. Bitte versuche es
                erneut.
            </InlineNotification>
        {:else}
            <InlineNotification lowContrast kind="error" hideCloseButton>
                Ups, da ist wohl irgendwas schiefgelaufen. Versuche die Seite
                neu zu laden...
            </InlineNotification>
        {/if}
    {/if}
</WalterGrid>
