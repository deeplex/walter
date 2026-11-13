<!-- Copyright (C) 2023-2026  Kai Lawrence -->
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

<!--
    Ein Button „Rückgängig machen“ für eine Zahlung (Transaktion) oder einen einzelnen
    Buchungssatz. Was dabei passiert — vollständig entfernen oder per Gegenbuchung —
    entscheidet der Server; der Dialog fragt nur, was dafür nötig ist.
    Event `erledigt` nach dem Rückgängigmachen.
-->
<script lang="ts">
    import {
        Button,
        Checkbox,
        InlineLoading,
        InlineNotification,
        Modal,
        TextArea
    } from 'carbon-components-svelte';
    import { Undo } from 'carbon-icons-svelte';
    import type { RueckgaengigPlan } from '$walter/lib';
    import { walter_fetch, walter_post } from '$walter/services/requests';
    import { createEventDispatcher } from 'svelte';

    export let fetchImpl: typeof fetch;
    /** Transaktion; sonst `buchungssatzId`. */
    export let transaktionId: string | undefined = undefined;
    export let buchungssatzId: string | undefined = undefined;
    export let size: 'small' | 'field' | 'default' = 'small';
    export let kind: 'ghost' | 'danger-tertiary' = 'ghost';

    const dispatch = createEventDispatcher<{ erledigt: RueckgaengigPlan }>();

    $: url = transaktionId
        ? `/api/transaktionen/${transaktionId}/rueckgaengig`
        : `/api/buchungssaetze/${buchungssatzId}/rueckgaengig`;

    let open = false;
    let plan: RueckgaengigPlan | undefined;
    let grund = '';
    let bestaetigt = false;
    let busy = false;
    let fehler: string | null = null;

    async function oeffnen() {
        open = true;
        plan = undefined;
        grund = '';
        bestaetigt = false;
        fehler = null;
        const response = await walter_fetch(fetchImpl, url);
        if (response.ok) plan = (await response.json()) as RueckgaengigPlan;
        else fehler = await response.text();
    }

    $: braucheGrund = plan?.art === 'Storno';
    $: braucheBestaetigung = (plan?.abrechnungen.length ?? 0) > 0;
    $: ausfuehrbar =
        !!plan &&
        plan.art !== 'Gesperrt' &&
        (!braucheGrund || grund.trim().length > 0) &&
        (!braucheBestaetigung || bestaetigt);

    async function ausfuehren() {
        if (!ausfuehrbar) return;
        busy = true;
        fehler = null;
        try {
            const response = await walter_post(url, {
                grund: grund.trim() || null,
                abrechnungenBestaetigt: bestaetigt
            });
            if (response.ok) {
                open = false;
                dispatch(
                    'erledigt',
                    (await response.json()) as RueckgaengigPlan
                );
            } else {
                fehler = await response.text();
            }
        } finally {
            busy = false;
        }
    }
</script>

<Button {kind} {size} icon={Undo} on:click={oeffnen}>Rückgängig machen</Button>

<Modal
    bind:open
    danger={plan?.art !== 'Gesperrt'}
    modalHeading="Rückgängig machen"
    primaryButtonText={plan?.art === 'Gesperrt'
        ? 'Schließen'
        : 'Rückgängig machen'}
    secondaryButtonText="Abbrechen"
    primaryButtonDisabled={busy || (plan?.art !== 'Gesperrt' && !ausfuehrbar)}
    on:click:button--secondary={() => (open = false)}
    on:submit={() => (plan?.art === 'Gesperrt' ? (open = false) : ausfuehren())}
>
    {#if !plan && !fehler}
        <InlineLoading description="Prüfe…" />
    {:else if plan?.art === 'Gesperrt'}
        <p>Das lässt sich nicht rückgängig machen.</p>
        {#if plan.sperrgrund}<p class="hinweis">{plan.sperrgrund}</p>{/if}
    {:else if plan?.art === 'Loeschen'}
        <p>
            {transaktionId ? 'Die ganze Zahlung' : 'Die Buchung'} wird vollständig
            entfernt, als hätte es sie nie gegeben. Daran hängt nichts weiter.
        </p>
    {:else if plan?.art === 'Storno'}
        <p>
            Bereits abgeschlossen — {transaktionId
                ? 'die ganze Zahlung'
                : 'die Buchung'} wird durch eine Gegenbuchung rückgängig gemacht.
            Beides bleibt sichtbar.
        </p>
        <TextArea
            labelText="Grund (Pflicht)"
            placeholder="Warum wird das rückgängig gemacht?"
            bind:value={grund}
        />
    {/if}

    {#if plan && plan.abrechnungen.length > 0}
        <InlineNotification
            kind="warning"
            title="Abrechnungen sind betroffen"
            subtitle="Diese Abrechnungen werden mit zurückgenommen und müssen danach neu erstellt werden:"
            hideCloseButton
            lowContrast
        />
        <ul class="abrechnungen">
            {#each plan.abrechnungen as a}
                <li>
                    {a.bezeichnung}
                    {a.jahr}{#if a.versendet}
                        <span class="hinweis"> (bereits versendet)</span>{/if}
                </li>
            {/each}
        </ul>
        <Checkbox
            labelText="Ich habe verstanden, dass diese Abrechnungen zurückgenommen werden."
            bind:checked={bestaetigt}
        />
    {/if}

    {#if fehler}
        <InlineNotification
            kind="error"
            title="Nicht möglich:"
            subtitle={fehler}
            hideCloseButton
            lowContrast
        />
    {/if}
</Modal>

<style>
    .hinweis {
        color: var(--cds-text-secondary);
    }
    .abrechnungen {
        margin: 0.5rem 0 1rem 1rem;
        list-style: disc;
    }
    p {
        margin-bottom: 1rem;
    }
</style>
