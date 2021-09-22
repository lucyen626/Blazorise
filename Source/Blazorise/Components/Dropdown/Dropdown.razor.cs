﻿#region Using directives
using System;
using System.Collections.Generic;
using Blazorise.States;
using Blazorise.Utilities;
using Microsoft.AspNetCore.Components;
#endregion

namespace Blazorise
{
    /// <summary>
    /// Dropdown is toggleable, contextual overlay for displaying lists of links and more.
    /// </summary>
    public partial class Dropdown : BaseComponent
    {
        #region Members

        /// <summary>
        /// State object used to holds the dropdown state.
        /// </summary>
        private DropdownState state = new()
        {
            Direction = Direction.Down,
        };

        /// <summary>
        /// An event raised after the <see cref="Visible"/> parameter has changed.
        /// </summary>
        public event EventHandler<bool> VisibleChanged;

        /// <summary>
        /// A list of all buttons placed inside of this dropdown.
        /// </summary>
        private List<Button> buttonList;

        /// <summary>
        /// Tracks the last DropdownToggle Element Id that acted.
        /// </summary>
        public string SelectedDropdownElementId { get; set; }

        #endregion

        #region Methods        

        /// <inheritdoc/>
        protected override void BuildClasses( ClassBuilder builder )
        {
            builder.Append( ClassProvider.Dropdown() );
            builder.Append( ClassProvider.DropdownGroup(), IsGroup );
            builder.Append( ClassProvider.DropdownShow(), Visible );
            builder.Append( ClassProvider.DropdownRight(), RightAligned );
            builder.Append( ClassProvider.DropdownDirection( Direction ), Direction != Direction.Down );
            builder.Append( ClassProvider.DropdownTableResponsive(), InResponsiveTable );

            base.BuildClasses( builder );
        }

        /// <inheritdoc/>
        protected override void OnAfterRender( bool firstRender )
        {
            if ( firstRender && buttonList?.Count > 0 )
            {
                DirtyClasses();
                DirtyStyles();

                InvokeAsync( StateHasChanged );
            }

            WasJustToggled = false;

            base.OnAfterRender( firstRender );
        }

        /// <summary>
        /// Show the dropdown menu.
        /// </summary>
        public void Show()
        {
            // used to prevent toggle event call if Open() is called multiple times
            if ( Visible )
                return;

            Visible = true;

            InvokeAsync( StateHasChanged );
        }

        /// <summary>
        /// Hide the dropdown menu.
        /// </summary>
        public void Hide( bool hideAll = false )
        {
            // used to prevent toggle event call if Close() is called multiple times
            if ( !Visible )
                return;

            Visible = false;

            if ( ParentDropdown is not null && ( ParentDropdown.ShouldClose || hideAll ) )
                ParentDropdown.Hide( hideAll );

            InvokeAsync( StateHasChanged );
        }

        /// <summary>
        /// Keeps track whether the Dropdown is in a state where it should close.
        /// </summary>
        internal bool ShouldClose { get; set; } = false;

        /// <summary>
        /// Keeps track whether the Dropdown was just toggled, ignoring possible DropdownItem clicks which would otherwise close the dropdown.
        /// </summary>
        internal bool WasJustToggled { get; set; } = false;


        /// <summary>
        /// Handles the onmouseleave event.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected void MouseLeave()
        {
            ShouldClose = true;
        }


        /// <summary>
        /// Handles the onmouseenter event.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected void MouseEnter()
        {
            ShouldClose = false;
        }

        /// <summary>
        /// Toggle the visibility of the dropdown menu.
        /// </summary>
        public void Toggle( string dropdownToggleElementId )
        {
            SetWasJustToggled( true );
            SetSelectedDropdownElementId( dropdownToggleElementId );
            Visible = !Visible;

            InvokeAsync( StateHasChanged );
        }

        /// <summary>
        /// Sets the WasToggled Flag on the current Dropdown and every existing ParentDropdown.
        /// </summary>
        /// <param name="wasToggled"></param>
        internal void SetWasJustToggled( bool wasToggled )
        {
            WasJustToggled = wasToggled;
            ParentDropdown?.SetWasJustToggled( wasToggled );
        }

        internal void SetSelectedDropdownElementId( string dropdownToggleElementId )
        {
            SelectedDropdownElementId = dropdownToggleElementId;
            if ( ParentDropdown is not null )
                ParentDropdown.SetSelectedDropdownElementId( dropdownToggleElementId );
        }

        /// <summary>
        /// Notifies the <see cref="Dropdown"/> that it has a child button component.
        /// </summary>
        /// <param name="button">Reference to the <see cref="Button"/> that is placed inside of this <see cref="Dropdown"/>.</param>
        internal void NotifyButtonInitialized( Button button )
        {
            if ( button == null )
                return;

            buttonList ??= new();

            if ( !buttonList.Contains( button ) )
            {
                buttonList.Add( button );
            }
        }

        /// <summary>
        /// Notifies the <see cref="Dropdown"/> that it's a child button component should be removed.
        /// </summary>
        /// <param name="button">Reference to the <see cref="Button"/> that is placed inside of this <see cref="Dropdown"/>.</param>
        internal void NotifyButtonRemoved( Button button )
        {
            if ( button == null )
                return;

            if ( buttonList != null && buttonList.Contains( button ) )
            {
                buttonList.Remove( button );
            }
        }

        /// <summary>
        /// Handles the styles based on the visibility flag.
        /// </summary>
        /// <param name="visible">Dropdown menu visibility flag.</param>
        private void HandleVisibilityStyles( bool visible )
        {
            DirtyClasses();
            DirtyStyles();
        }

        /// <summary>
        /// Handles all the events in this <see cref="Dropdown"/> based on the visibility flag.
        /// </summary>
        /// <param name="visible">Dropdown menu visibility flag.</param>
        private void HandleVisibilityEvents( bool visible )
        {
            VisibleChanged?.Invoke( this, visible );

            Toggled.InvokeAsync( visible );
        }

        #endregion

        #region Properties

        /// <inheritdoc/>
        protected override bool ShouldAutoGenerateId => true;

        /// <summary>
        /// Gets the reference to the <see cref="DropdownState"/>.
        /// </summary>
        protected DropdownState State => state;

        /// <summary>
        /// Makes the drop down to behave as a group for buttons(used for the split-button behaviour).
        /// </summary>
        protected internal bool IsGroup => ParentButtons != null || buttonList?.Count >= 1;

        /// <summary>
        /// Returns true if dropdown is placed inside of responsive table.
        /// </summary>
        protected internal bool InResponsiveTable => ParentTable?.Responsive == true;

        /// <summary>
        /// If true, a dropdown menu will be visible.
        /// </summary>
        [Parameter]
        public bool Visible
        {
            get => state.Visible;
            set
            {
                // prevent from calling the same code multiple times
                if ( value == state.Visible )
                    return;

                state = state with { Visible = value };

                HandleVisibilityStyles( value );
                HandleVisibilityEvents( value );
            }
        }

        /// <summary>
        /// If true, a dropdown menu will be right aligned.
        /// </summary>
        [Parameter]
        public bool RightAligned
        {
            get => state.RightAligned;
            set
            {
                state = state with { RightAligned = value };

                DirtyClasses();
            }
        }

        /// <summary>
        /// If true, dropdown would not react to button click.
        /// </summary>
        [Parameter]
        public bool Disabled
        {
            get => state.Disabled;
            set
            {
                state = state with { Disabled = value };

                DirtyClasses();
            }
        }

        /// <summary>
        /// Dropdown-menu slide direction.
        /// </summary>
        [Parameter]
        public Direction Direction
        {
            get => state.Direction;
            set
            {
                state = state with { Direction = value };

                DirtyClasses();
            }
        }

        /// <summary>
        /// Occurs after the dropdown menu visibility has changed.
        /// </summary>
        [Parameter] public EventCallback<bool> Toggled { get; set; }

        /// <summary>
        /// Gets or sets the cascaded parent buttons component.
        /// </summary>
        [CascadingParameter] protected Buttons ParentButtons { get; set; }

        /// <summary>
        /// Gets or sets the cascaded parent table component.
        /// </summary>
        [CascadingParameter] protected Table ParentTable { get; set; }

        /// <summary>
        /// Gets or sets the cascaded parent Dropdown component.
        /// </summary>
        [CascadingParameter] protected Dropdown ParentDropdown { get; set; }

        /// <summary>
        /// Specifies the content to be rendered inside this <see cref="Dropdown"/>.
        /// </summary>
        [Parameter] public RenderFragment ChildContent { get; set; }

        #endregion
    }
}
