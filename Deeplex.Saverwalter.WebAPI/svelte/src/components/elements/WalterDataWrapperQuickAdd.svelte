<!-- Copyright (C) 2023-2024  Kai Lawrence -->
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
    import { Modal } from 'carbon-components-svelte';
    import { handle_save } from './WalterDataWrapper';
    import { changeTracker } from '$walter/store';
    import { get } from 'svelte/store';

    export let addModalOpen = false;
    export let addUrl: string;
    export let addEntry: unknown;
    export let title: string | undefined = undefined;
    export let onSubmit: undefined | ((e: unknown) => void) = undefined;

    async function submit() {
        if (!addUrl) return;

        const parsed = await handle_save(addUrl, addEntry, title!);

        if (onSubmit) {
            onSubmit(parsed);
        }
    }

    let tracker: number;
    function open() {
        tracker = get(changeTracker);
    }

    function close() {
        addModalOpen = false;
        changeTracker.set(tracker);
    }
</script>

<Modal
    style="width: 100%; height: 100%;"
    size="lg"
    secondaryButtonText="Abbrechen"
    primaryButtonText="Bestätigen"
    on:open={open}
    on:close={close}
    on:submit={submit}
    on:click:button--secondary={() => (addModalOpen = false)}
    on:click:button--primary={() => (addModalOpen = false)}
    modalHeading={title
        ? `Eintrag zu ${title} hinzufügen`
        : `Eintrag hinzufügen`}
    bind:open={addModalOpen}
    primaryButtonDisabled={!addUrl}
    hasScrollingContent
>
    <slot />
    <!-- Spacer for DatePickers. Otherwise the modal is too narrow -->
    <div style="height: 13em" />
</Modal>
